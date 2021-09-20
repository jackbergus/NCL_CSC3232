//
// Created by giacomo on 28/08/21.
//

#include <string>
#include <unordered_map>
#include <unordered_set>
#include <vector>
#include <ostream>
#include <iostream>

struct action {
    std::string name;
    double      probability;
    double      reward;

    action() : action("noop", 1.0, 0) {}
    action(const std::string &name, double probability, double reward) : name(name), probability(probability),
                                                                         reward(reward) {}
    action(const action& ) = default;
    action(action&& ) = default;
    action& operator=(const action& ) = default;
    action& operator=(action&& ) = default;

    friend std::ostream &operator<<(std::ostream &os, const action &action) {
        os << "name: " << action.name << " probability: " << action.probability << " reward: " << action.reward;
        return os;
    }
};

struct stateful_graph {
    std::unordered_set<std::string> allStates;
    std::unordered_map<std::string, std::unordered_map<std::string, std::unordered_map<std::string, action>>> adjacency_graph;
    std::unordered_set<std::string> accepting_states;
    std::string initial_state;

    stateful_graph() = default;
    stateful_graph(const stateful_graph& ) = default;
    stateful_graph(stateful_graph&& ) = default;
    stateful_graph& operator=(const stateful_graph& ) = default;
    stateful_graph& operator=(stateful_graph&& ) = default;

    std::unordered_set<std::string> getOutgoingActionNames(const std::string& stateName) const {
        std::unordered_set<std::string> actionSet;
        auto cp = adjacency_graph.find(stateName);
        if (cp != adjacency_graph.end())
        for (const auto& adj : cp->second) {
            for (const auto& action : adj.second) {
                actionSet.insert(action.first);
            }
        }
        return actionSet;
    }

    double getCost(double gamma, double dstV, const std::string& src, const std::string& dst, const std::string& stateName) const {
        auto it = adjacency_graph.find(src);
        if (it == adjacency_graph.end()) return 0.0;
        auto it2 = it->second.find(dst);
        if (it2 == it->second.end()) return 0.0;
        auto it3 = it2->second.find(stateName);
        if (it3 == it2->second.end()) return 0.0;
        else return it3->second.probability * (it3->second.reward + gamma * dstV);
    }

};

#include <random>

struct PolicyIteration {
    const stateful_graph& G;
    std::unordered_map<std::string, double> V;      // Greedy value associated to each state
    std::unordered_map<std::string, std::unordered_map<std::string, double>> policy;
    std::unordered_map<std::string, std::string> det_policy;
    const double gamma;

    PolicyIteration(const stateful_graph &g, double gamma) : G(g), gamma{gamma} {
        double lower_bound = 0;
        double upper_bound = 100;
        std::uniform_real_distribution<double> unif(lower_bound,upper_bound);
        std::default_random_engine re;
        for (const auto& cp : G.allStates)
            if (G.accepting_states.contains(cp))
                V[cp] = 0.0;
            else
                V[cp] = unif(re);

    }

    void loop(double theta) {
        double Delta;
        do {
            Delta = 0.0;
            for (const auto& s : G.allStates) {
                double v = V.at(s);
                double argMax = -std::numeric_limits<double>::max();
                auto res = G.getOutgoingActionNames(s);
                if (!res.empty()) {
                    for (const auto& actionName : res) {
                        double sum = 0.0;
                        for (const auto& sp : G.allStates) {
                            sum += G.getCost(gamma, V.at(sp), s, sp, actionName);
                        }
                        if (sum >= argMax) {
                            argMax = sum;
                        }
                    }
                    V[s] = argMax;
                    Delta = std::max(Delta, std::abs(v - argMax));
                }
            }
        } while (Delta > theta);

        for (const auto& s : G.allStates) {
            double argMax = -std::numeric_limits<double>::max();
            std::string argName = "";
            for (const auto& actionName : G.getOutgoingActionNames(s)) {
                double sum = 0.0;
                for (const auto& sp : G.allStates) {
                    sum += G.getCost(gamma, V.at(sp), s, sp, actionName);
                }
                policy[s][actionName] = sum;
                if (sum >= argMax) {
                    argMax = sum;
                    argName = actionName;
                }
            }
            det_policy[s] = argName;
        }
    }
};

int main(void) {
    std::string reading_1 = "ReadingDay1";
    std::string reading_2 = "ReadingDay2";
    std::string reading_3 = "ReadingDay3";
    std::string party_1 = "Party1";
    std::string party_2 = "Party2";
    std::string party_3 = "Party3";
    std::string passed = "PassedExam";
    stateful_graph G;
    G.allStates = {reading_1, reading_2, reading_3, party_1, party_2, party_3, passed};
    G.adjacency_graph[reading_1][reading_2]["study"] = {"study", 0.7, -2.0};
    G.adjacency_graph[reading_1][party_1]["party!"] = {"party!", 0.3, 1.0};
    G.adjacency_graph[party_1][reading_1]["headache"] = {"headache", 1.0, -2.0};

    G.adjacency_graph[reading_2][reading_3]["study"] = {"study", 0.8, -2.0};
    G.adjacency_graph[reading_2][party_2]["party!"] = {"party!", 0.2, 1.0};
    G.adjacency_graph[party_2][reading_2]["headache"] = {"headache", 0.8, -1.0};
    G.adjacency_graph[party_2][party_1]["strong headache"] = {"strong headache", 0.2, -1.0};

    G.adjacency_graph[reading_3][passed]["study&pass"] = {"study&pass", 0.9, 10.0};
    G.adjacency_graph[reading_3][party_3]["party!"] = {"party!", 0.1, 1.0};
    G.adjacency_graph[party_3][reading_3]["headache"] = {"headache", 0.8, -1.0};
    G.adjacency_graph[party_3][party_2]["strong headache"] = {"strong headache", 0.2, -1.0};
    G.accepting_states.insert(passed);

    PolicyIteration policyIteration(G, 0.5);
    policyIteration.loop(0.01);

    for (const auto& cp : policyIteration.V)
        std::cout << " Value (" << cp.first << ")= " << cp.second << std::endl;

    for (const auto& cp : policyIteration.det_policy)
        std::cout << " Pi(" << cp.first << ")= " << cp.second << std::endl;

    for (const auto& cp : policyIteration.policy)
        for (const auto& cp2 : cp.second)
            std::cout << " Pi(" << cp.first << "|" << cp2.first << ")= " << cp2.second << std::endl;
}