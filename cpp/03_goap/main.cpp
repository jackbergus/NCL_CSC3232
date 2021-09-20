#include <iostream>

#include <yaucl/hashing/hash_combine.h>
#include <yaucl/hashing/pair_hash.h>
#include <yaucl/hashing/uset_hash.h>

template <typename K, typename V>
std::ostream& operator<< (std::ostream& out, const std::pair<K, V>& v) {
    return out << "«" << v.first << ", " << v.second << "»";
}

template <typename K>
std::ostream& operator<< (std::ostream& out, const std::unordered_set<K>& v) {
    out << "{";
    auto it = v.begin();
    while (it != v.end()) {
        out << *it;
        it++;
        if (it != v.end())
            out << ", ";
    }
    return out << "}";
}

template <typename T>
bool isSubsetOf(const std::unordered_set<T>& subset, const std::unordered_set<T>& supset) {
    for (const auto& x : subset)
        if (!supset.contains(x))
            return false;
    return true;
}



template <typename T>
bool solvability_test(const std::unordered_set<T>& init,
                      const std::unordered_set<T>& goal,
                      const std::unordered_set<std::pair<std::unordered_set<T>, T>>& rules) {
    if (isSubsetOf(goal, init))
        return true; // already solved!
    else {
        std::unordered_set<T> S{init.begin(), init.end()};
        std::unordered_set<T> tmp{init.begin(), init.end()};
        std::cout << "S = " << S << std::endl;
        do {
            S = tmp;
            for (const auto& cp : rules) {
                if (isSubsetOf(cp.first, tmp) && (!tmp.contains(cp.second))) {
                    std::cout << "- Applying rule: " << cp << std::endl;
                    tmp.insert(cp.second);
                    std::cout << "  * State becomes: " << tmp << std::endl;
                }
            }
        } while ((!isSubsetOf(S, tmp)) || (!isSubsetOf(tmp, S)));
        return (isSubsetOf(goal, tmp));
    }
}

#include <cassert>
using state = std::unordered_set<std::string>;
using rule = std::pair<state, std::string>;

void preliminary_test() {
    assert(solvability_test<std::string>({"A"}, {"A"}, {}));
    assert(!solvability_test<std::string>({"A"}, {"B"}, {}));
    assert(!solvability_test<std::string>({"A"}, {"B"}, {{{"C"}, "D"}}));
    assert(solvability_test<std::string>({"A"}, {"B"}, {{{"A"}, "B"}}));

}

#include <unordered_map>
#include <vector>

struct stateful_graph {
    std::unordered_map<state, std::unordered_map<state, std::unordered_set<rule>>> adjacency_graph;
    std::unordered_set<state> accepting_states;
    state initial_state;
    std::vector<std::string> errors;

    stateful_graph() = default;
    stateful_graph(const stateful_graph& ) = default;
    stateful_graph(stateful_graph&& ) = default;
    stateful_graph& operator=(const stateful_graph& ) = default;
    stateful_graph& operator=(stateful_graph&& ) = default;

    friend std::ostream &operator<<(std::ostream &os, const stateful_graph &graph) {
        for (const auto& cpM : graph.adjacency_graph) {
            for (const auto& cp2M : cpM.second) {
                for (const auto& rule : cp2M.second) {
                    os << cpM.first << "--[" << rule << "]-->" << cp2M.first << std::endl;
                }
            }
        }
        os << "Starting: " << graph.initial_state << std::endl;
        os << "Accepting: " << graph.accepting_states << std::endl;
        return os;
    }

    void dot(std::ostream &os) const {
        os << "digraph finite_state_machine {\n"
              "    rankdir=LR;\n"
              "    size=\"8,5\"\n";
        int node_id = 0;
        std::unordered_map<state, size_t> M;
        for (const auto& it : adjacency_graph) {
            std::string shape = "circle";
            os << "node [shape = circle, label=\"" << it.first << "\", fontsize=10] q" << node_id << ";\n";
            M[it.first] = node_id++;
        }
        os << "\n\n";
        for (const auto& it : adjacency_graph) {
            for (const auto& multiedge_id : it.second) {
                os << "q" << M[it.first] << " -> q" << M[multiedge_id.first] << ";\n";
            }
        }
        os << "}";
    }
};

state DFSGeneratePossibleStates(stateful_graph& G,
                                const state& S,
                                const state& Goal,
                                const std::unordered_set<rule>& Rules) {
    if (G.adjacency_graph.contains(S)) return S;
    else {
        G.adjacency_graph[S] = {};
        if (isSubsetOf(Goal, S))
            G.accepting_states.insert(S);
        for (const auto& cp : Rules) {
            if (isSubsetOf(cp.first, S) && (!S.contains(cp.second))) {
                state tmp{S.begin(), S.end()};
                tmp.insert(cp.second);
                auto S2 = DFSGeneratePossibleStates(G, tmp, Goal, Rules);
                G.adjacency_graph[S][S2].insert(cp);
            }
        }
        return S;
    }
}

#include <vector>
#include <sstream>

struct GenerateBacktrackStates {
    std::vector<stateful_graph> graphs;

    void generate_graphs(const state& Goal,
                         const state& Init,
                         const std::unordered_set<rule>& Rules) {
        graphs.emplace_back();
        if (Goal.size() == 1) {
            DFSGenerateBacktrackStates(0, *Goal.begin(), Init, Rules);
        } else {
            graphs[0].initial_state = Goal;
            graphs[0].adjacency_graph[Goal] = {};
            for (const std::string& x : Goal) {
                auto S2 = DFSGenerateBacktrackStates(0, x, Init, Rules);
                std::stringstream ss ;
                ss << "Error on expanding for: " << x << " over precondition = " << x;
                std::string y  = ss.str();
                for (size_t id : S2.second) {
                    graphs[id].adjacency_graph[Goal][S2.first].insert({{}, x});
                    if (S2.first.empty()) {
                        graphs[id].errors.emplace_back(y);
                    }
                }
            }
        }

    }


private:
    std::pair<state, std::vector<size_t>> DFSGenerateBacktrackStates(size_t idG,
                                     const std::string& s,
                                     const state& Init,
                                     const std::unordered_set<rule>& Rules) {
        state S = {s};
        if (graphs[idG].adjacency_graph.contains(S)) return {S, {idG}};
        else {
            std::vector<size_t> resulting_graphs{idG};
            graphs[idG].adjacency_graph[S] = {};
            if (Init.contains(s))
                graphs[idG].accepting_states.insert(S);
            else {
                size_t countFoundAlsoPartial = 0;
                stateful_graph copyGraph = graphs[idG];
                for (const auto& cp : Rules) {
                    if (cp.second == s) {
                        countFoundAlsoPartial++;
                        if (countFoundAlsoPartial == 1) {
                            for (const std::string& x : cp.first) {
                                auto S2 = DFSGenerateBacktrackStates(idG, x, Init, Rules);
                                std::stringstream ss;
                                ss << "Error on applying rule: " << cp << " over precondition = " << x << std::endl;
                                std::string y = ss.str();
                                for (size_t id : S2.second) {
                                    graphs[id].adjacency_graph[S][S2.first].insert(cp);
                                    if (S2.first.empty()) {
                                        graphs[id].errors.emplace_back(y);
                                    }
                                }

                            }
                        } else {
                            size_t currSize = graphs.size();
                            resulting_graphs.emplace_back(currSize);
                            graphs.emplace_back(copyGraph);
                            for (const std::string& x : cp.first) {
                                auto S2 = DFSGenerateBacktrackStates(currSize, x, Init, Rules);
                                graphs[currSize].adjacency_graph[S][S2.first].insert(cp);
                                std::stringstream ss;
                                ss << "Error on applying rule: " << cp << " over precondition = " << x << std::endl;
                                std::string y = ss.str();
                                for (size_t id : S2.second) {
                                    graphs[id].adjacency_graph[S][S2.first].insert(cp);
                                    if (S2.first.empty()) {
                                        graphs[id].errors.emplace_back(y);
                                    }
                                }
                            }
                        }

                    }
                }
                if (countFoundAlsoPartial == 0) {
                    std::stringstream ss;
                    ss << "Error: it was not possible to apply a rule for configuration = " << s;
                    std::string x = ss.str();
                    graphs[idG].errors.emplace_back(ss.str());
                    return {{}, resulting_graphs};
                }
            }
            return {S, resulting_graphs};
        }
    }
};



void example(bool single_path_example = true,
             bool single_path_with_errors = true,
             bool generate_possible_states = false,
             bool check_solvability = false) {
    std::string key_a = "Key_A";
    std::string key_b = "Key_B";
    std::string key_c = "Key_C";
    std::string key_d = "Key_D";
    std::string key_e = "Key_E";
    std::string key_f = "Key_Finish";
    std::string key_t = "Key_T";
    std::string door_a = "Door_A";
    std::string door_b = "Door_B";
    std::string door_ce = "Door_CE";
    std::string door_d1 = "Door_D1";
    std::string door_d2 = "Door_D2";
    std::string door_t = "Door_T";
    std::string door_f = "Door_Finish";

    rule r1 = {{key_a}, {door_a}};
    rule r2a = {{door_a}, {key_e}};
    rule r2b = {{door_a}, {key_d}};
    rule r3a = {{door_a, key_d}, {door_d2}};
    rule r3b = {{key_d}, {door_d1}};
    rule r4 = {{door_a, key_t}, {door_t}};
    rule r5 = {{door_d2}, {key_t}};
    rule r6 = {{door_t}, {key_c}};
    rule r7 = {{door_d1}, {key_b}};
    rule r8 = {{door_d1, key_b}, {door_b}};
    rule r9 = {{door_b}, {key_f}};
    rule r10 = {{key_c, key_e}, {door_ce}};
    rule r11 = {{door_ce, key_f}, {door_f}};

    if (check_solvability) {
        assert(solvability_test<std::string>({key_a}, {door_f}, {r1, r2a, r2b, r3a, r3b, r4, r5, r6, r7, r8, r9, r10, r11}));
    }
    if (generate_possible_states) {
        stateful_graph G;
        G.initial_state = {key_a};
        DFSGeneratePossibleStates(G, G.initial_state, {door_f}, {r1, r2a, r2b, r3a, r3b, r4, r5, r6, r7, r8, r9, r10, r11});
    }

    GenerateBacktrackStates gbs;

    if (single_path_example) {
        if (!single_path_with_errors) {
            gbs.generate_graphs({door_f}, {key_a}, {r1, r2a, r2b, r3a, r3b, r4, r5, r6, r7, r8, r9, r10, r11});
        } else {
            gbs.generate_graphs({door_f}, {key_a}, {r1, r2a, r2b, r3a, r3b, r4, r5, r6, r8, r9, r10, r11});
        }
    } else {
        rule r6b = {{door_t}, {key_f}};
        rule r9b = {{door_b}, {key_c}};
        rule r11b = {{door_a, door_t, key_f}, {door_f}};
        rule r11c = {{door_d1, door_b, key_f}, {door_f}};
        gbs.generate_graphs({door_f}, {key_a}, {r1, r2a, r2b, r3a, r3b, r4, r5, r6, r7, r8, r9, r10, r11, r6b, r9b, r11b, r11c});
    }


    for (size_t i = 0, N = gbs.graphs.size(); i<N; i++) {
        const auto& G = gbs.graphs.at(i);
        std::cout << "==========================================================" << std::endl;
        std::cout << " Graph #" << i << std::endl<< std::endl;
        if (!G.errors.empty()) {
            std::cout.flush();
            for (const std::string& error : G.errors)
                std::cerr << error << std::endl;
            std::cerr.flush();
        }
        G.dot(std::cout);
        std::cout << std::endl << "==========================================================" << std::endl;
    }
}





int main() {
    example();

     return 0;
}
