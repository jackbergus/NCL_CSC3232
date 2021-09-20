//
// Created by giacomo on 26/08/21.
//

#include <cstddef>
#include <utility>
#include <cmath>


/**
 * Computes the distance between two coordinate
 * @param p1    Coordinate 1
 * @param p2    Coordinate 2
 * @return  Distance between the coordinates
 */
double pairDistance(const std::pair<size_t, size_t> &p1,
                    const std::pair<size_t, size_t> &p2) {
    return std::sqrt(std::pow(((double) p1.first) - ((double) p2.first), 2) +
                     std::pow(((double) p1.second) - ((double) p2.second), 2));
}


enum Directions {
    N = 3,
    //NE = 4,
    E = 4,
    //SE = 6,
    S = 5,
    //SW = 8,
    W = 6,
    //NW = 10
};

/**
 * Performs the move from the cell on the current coordinate towards the cell indicated by the direction
 * @param envStatus     Status containing the current location
 * @param dir           Direction of movement
 * @return              If the movement is allowed, then first element of the pair is true and the second element
 *                      is the new cell location, and otherwise is false and the seocnd element is just the origin
 */
std::pair<bool,
        std::pair<size_t, size_t>> move(const std::pair<size_t, size_t> &currentCellCoord,
                                        Directions dir,
                                        size_t maxBoardSizeX,
                                        size_t maxBoardSizeY) {
    long long x = currentCellCoord.first;
    long long y = currentCellCoord.second;
    switch (dir) {
        case N:
            y++;
            break;
        case S:
            y--;
            break;
        case W:
            x--;
            break;
        case E:
            x++;
            break;
        /*case NW:
            y++;
            x--;
            break;
        case SW:
            y--;
            x--;
            break;
        case NE:
            y++;
            x++;
            break;
        case SE:
            y--;
            x++;
            break;*/
    }
    if ((x >= 0) && (y >= 0) && (x < maxBoardSizeX) && (y < maxBoardSizeY)) {
        return {true, {x, y}};
    } else {
        return {false, currentCellCoord};
    }
}

#include <vector>
#include <ostream>

template <typename K, typename V>
std::ostream& operator<< (std::ostream& out, const std::pair<K, V>& v) {
    return out << "«" << v.first << ", " << v.second << "»";
}

#include <unordered_set>

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

template <typename K>
std::ostream& operator<< (std::ostream& out, const std::vector<K>& v) {
    out << "[[";
    auto it = v.begin();
    while (it != v.end()) {
        out << *it;
        it++;
        if (it != v.end())
            out << "; ";
    }
    return out << "]]";
}

std::vector<std::pair<Directions, std::pair<size_t, size_t>>> generateDirections(const std::pair<size_t, size_t>& current,
                                                          size_t maxBoardSizeX, size_t maxBoardSizeY) {

    std::vector<std::pair<Directions, std::pair<size_t, size_t>>> allowedDirections;
    for (int dir = (int)Directions::N; dir != ((int)Directions::W) + 1; dir++ ) {
        Directions mov = static_cast<Directions>(dir);
        auto M = move(current, mov, maxBoardSizeX, maxBoardSizeY);
        if (M.first) {
            allowedDirections.emplace_back(mov, M.second);
        }
    }
    return allowedDirections;
}

std::vector<std::pair<Directions, std::pair<size_t, size_t>>> rankDirections(const std::vector<std::pair<Directions, std::pair<size_t, size_t>>>& toRank,
                                       const std::pair<size_t, size_t> &desirableTarget) {

    std::vector<std::pair<Directions, std::pair<size_t, size_t>>> cp = toRank;
    std::sort(cp.begin(), cp.end(), [desirableTarget](const auto &x, const auto &y) { return pairDistance(x.second, desirableTarget) - pairDistance(y.second, desirableTarget); });
    return cp;
}


enum LoadType {
    OneLog = 2,
    OneStone = 1,
    FuelOrNoop = 0
};

#include <yaucl/hashing/hash_combine.h>
#include <yaucl/hashing/pair_hash.h>
#include <yaucl/hashing/vector_hash.h>

struct EnvironmentStatus {
    double satiety;
    double remaining_time;
    size_t game_progress;
    size_t nActionsPerformed;
    LoadType isLoadedOrEmpty;
    std::pair<size_t, size_t> currentCellCoord;
    std::vector<size_t> LogCellsContent;
    std::vector<size_t> StoneCellsContent;
    std::vector<LoadType> UnloadZoneContent;
    bool isIgnited;

    EnvironmentStatus() : EnvironmentStatus{0, 0} {}
    EnvironmentStatus(size_t x, size_t y) : nActionsPerformed{0}, satiety{30}, remaining_time{100.0}, game_progress{0},
                                            isLoadedOrEmpty{FuelOrNoop}, isIgnited{false},
                                            currentCellCoord{x, y} {};
    EnvironmentStatus(EnvironmentStatus &&) = default;
    EnvironmentStatus(const EnvironmentStatus &) = default;
    EnvironmentStatus &operator=(EnvironmentStatus &&) = default;
    EnvironmentStatus &operator=(const EnvironmentStatus &) = default;

    bool operator==(const EnvironmentStatus &rhs) const {
        return satiety == rhs.satiety &&
               remaining_time == rhs.remaining_time &&
               game_progress == rhs.game_progress &&
               isLoadedOrEmpty == rhs.isLoadedOrEmpty &&
               currentCellCoord == rhs.currentCellCoord &&
               LogCellsContent == rhs.LogCellsContent &&
               StoneCellsContent == rhs.StoneCellsContent &&
               UnloadZoneContent == rhs.UnloadZoneContent;
    }
    bool operator!=(const EnvironmentStatus &rhs) const {
        return !(rhs == *this);
    }

    bool isRightAmount() const {
        size_t countTotalLogs = 0;
        size_t countTotalStones = 0;
        size_t countTotalFuel = 0;
        for (const LoadType t : UnloadZoneContent) {
            if (t == LoadType::OneLog)
                countTotalLogs++;
            else if (t == LoadType::OneStone)
                countTotalStones++;
            else if (t == LoadType::FuelOrNoop)
                countTotalFuel++;
        }
        return (countTotalLogs>=2) && (countTotalStones>=3) && (countTotalFuel>=1);
    }

    bool isExactAmount() const {
        size_t countTotalLogs = 0;
        size_t countTotalStones = 0;
        size_t countTotalFuel = 0;
        for (const LoadType t : UnloadZoneContent) {
            if (t == LoadType::OneLog)
                countTotalLogs++;
            else if (t == LoadType::OneStone)
                countTotalStones++;
            else if (t == LoadType::FuelOrNoop)
                countTotalFuel++;
        }
        return countTotalLogs >= 1;//(countTotalLogs==2) && (countTotalStones==3);
    }

    bool isGameProgressPositive() const {
        size_t countTotalLogs = 0;
        size_t countTotalStones = 0;
        size_t countTotalFuel = 0;
        for (const LoadType t : UnloadZoneContent) {
            if (t == LoadType::OneLog)
                countTotalLogs++;
            else if (t == LoadType::OneStone)
                countTotalStones++;
            else if (t == LoadType::FuelOrNoop)
                countTotalFuel++;
        }
        return (countTotalLogs<=2) && (countTotalStones<=3) && (countTotalFuel<=1);
    }

    friend std::ostream &operator<<(std::ostream &os, const EnvironmentStatus &status) {
        os << "(satiety: " << status.satiety << " remaining_time: " << status.remaining_time << " game_progress: "
           << status.game_progress << " isLoadedOrEmpty: " << status.isLoadedOrEmpty << " currentCellCoord: "
           << status.currentCellCoord << " LogCellsContent: " << status.LogCellsContent << " StoneCellsContent: "
           << status.StoneCellsContent << " UnloadZoneContent: " << status.UnloadZoneContent << " isIgnited: "
           << status.isIgnited << ')';
        return os /*<< status.currentCellCoord*/;
    }

};

namespace std {
    template<>
    struct hash<EnvironmentStatus> {
        size_t operator()(const EnvironmentStatus &x) const {
            return yaucl::hashing::hash_combine(
                    yaucl::hashing::hash_combine(
                            yaucl::hashing::hash_combine(
                                    yaucl::hashing::hash_combine(
                                            yaucl::hashing::hash_combine(
                                                    yaucl::hashing::hash_combine(yaucl::hashing::hash_combine(
                                                            yaucl::hashing::hash_combine(31, x.satiety),
                                                            x.remaining_time), x.game_progress),
                                                    x.isLoadedOrEmpty),
                                            x.currentCellCoord),
                                    x.LogCellsContent),
                            x.StoneCellsContent),
                    x.UnloadZoneContent);
        }
    };
}

#include <functional>

using Predicate = std::function<bool(const struct EnvironmentStatus &)>;
using StatusUpdate = std::function<EnvironmentStatus(EnvironmentStatus)>;

enum RuleCases {
    LoadResource = 101,
    UnloadResource = 102,
    GainEnergy = 103,
    Move = 104,
    FastMove = 105,
    Ignite = 106,
    NOOP = 100
};

struct SerializableRule {
    RuleCases casus;
    Directions movement;
    bool       isMovementFast;
    double     probability;
    double     feedback;

    SerializableRule(RuleCases casus) : SerializableRule(casus, N, false) {}
    SerializableRule(RuleCases casus, Directions movement, bool isMovementFast) : casus(casus), movement(movement),
                                                                                  isMovementFast(isMovementFast),
                                                                                  probability{0.0}, feedback{0.0} {}
    SerializableRule() :casus{NOOP}, movement{N}, isMovementFast{false} {}
    SerializableRule(const SerializableRule&) = default;
    SerializableRule(SerializableRule&&) = default;
    SerializableRule& operator=(const SerializableRule&) = default;
    SerializableRule& operator=(SerializableRule&&) = default;

    friend std::ostream &operator<<(std::ostream &os, const SerializableRule &rule) {
        os << "casus: " << rule.casus << " movement: " << rule.movement << " isMovementFast: " << rule.isMovementFast
           << " probability: " << rule.probability << " feedback: " << rule.feedback;
        return os;
    }
};

struct stateful_graph {
    std::unordered_map<EnvironmentStatus, std::unordered_map<EnvironmentStatus, std::vector<SerializableRule>>> adjacency_graph;
    std::unordered_set<EnvironmentStatus> accepting_states, failing_states;
    EnvironmentStatus initial_state;
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
        std::unordered_map<EnvironmentStatus, size_t> M;
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



#include <cassert>
#include <ostream>
#include <iostream>

struct Board {
    struct EnvironmentStatus envStatus;
    std::pair<size_t, size_t> boardSize;
    std::pair<size_t, size_t> unloadingCoordinate;
    std::pair<size_t, size_t> fillingStationCoordinate;
    double gameProgressWeight;
    double timeWeight;
    double hungerWeight;
    double maxSatiety;
    double maxTime;
    std::vector<std::pair<size_t, size_t>> LogCellsPosition, StoneCellsPosition;

    double EatVsUnloadPreferrance = 0.8;
    double EatAndUnloadVsRest = 0.7;
    bool debug;

    Board(size_t maxX,
          size_t maxY,

          size_t botX,
          size_t botY,

          size_t unloadingCoordinateX = 0,
          size_t unloadingCoordinateY = 0,

          size_t fillingCoordinateX = 0,
          size_t fillingCoordinateY = 0,

          double maxSatiety = 20,
          double maxTime = 60) :
            debug{false},
            boardSize{maxX, maxY},
            envStatus{botX, botY},
            hungerWeight{0.2},
            timeWeight{0.1},
            maxSatiety{maxSatiety},
            maxTime{maxTime},
            gameProgressWeight{5.0},
            unloadingCoordinate{unloadingCoordinateX, unloadingCoordinateY},
            fillingStationCoordinate{fillingCoordinateX, fillingCoordinateY} {
        assert(botX < maxX);
        assert(botY < maxY);
        envStatus.satiety = maxSatiety;
        envStatus.remaining_time = maxTime;
    }

    void addLogCell(size_t x, size_t y, size_t start_quantity) {
        LogCellsPosition.emplace_back(x, y);
        envStatus.LogCellsContent.emplace_back(start_quantity);
    }

    void addStoneCell(size_t x, size_t y, size_t start_quantity) {
        StoneCellsPosition.emplace_back(x, y);
        envStatus.StoneCellsContent.emplace_back(start_quantity);
    }

    /**
     * Computing the expected reward from the state transition
     *
     * @param prev     Previous state
     * @param current  Current state
     * @return The reward to be associated for the transition
     */
    double countWeightDifference(const struct EnvironmentStatus &prev,
                                 const struct EnvironmentStatus &current) {
        return hungerWeight * (((double) current.satiety) - ((double) prev.satiety)) +
               timeWeight * (((double) current.remaining_time) - ((double) prev.remaining_time)) +
               gameProgressWeight * (((double) current.game_progress) - ((double) prev.game_progress));
    }

    stateful_graph generatePossibleStates(std::ostream& os) {
        stateful_graph G;
        DFSGeneratePossibleStates(os, G, envStatus, envStatus.currentCellCoord);
        return G;
    }

private:

    std::unordered_map<EnvironmentStatus, size_t> M;
    size_t generateStateId(const EnvironmentStatus& S) {
        size_t Sx = 0;
        auto it = M.find(S);
        if (it == M.end()) {
            M[S] = Sx = M.size();
        } else {
            Sx = it->second;
        }
        return Sx;
    }

    void DFSGeneratePossibleStates(std::ostream& os, stateful_graph& G, const EnvironmentStatus& S, const std::pair<size_t, size_t>& prevCell) {
        if (G.adjacency_graph.contains(S)) {
            size_t srcId = generateStateId(S);
            ///std::cout << " Already Met = " << srcId << std::endl;
            return;
        }
        else {
            size_t srcId = generateStateId(S);
            /*if (srcId >500000) {
                os.flush();
                return;
            }*/
            ///std::cout << " * " << srcId << std::endl;
            size_t dstId;

            //std::cout << S << std::endl;
            G.adjacency_graph[S] = {};
            if (S.isIgnited && (S.remaining_time >= 0)) {
                os << "Accepting state is reached! " << srcId << " with remaining time " << S.remaining_time << " and food " << S.satiety << std::endl;
                G.accepting_states.insert(S);       // Finishing the game, if in the former state I ignited.
            } else if ((S.remaining_time <= 0)) {
                ///os << "Failing state is reached! " << srcId << std::endl;
                G.failing_states.insert(S);         // Otherwise, at this time no further action is allowed
            } else if (S.satiety == 0)  {
                G.failing_states.insert(S);         // If I ran out of fuel, then I also lose the game
            } else {
                bool preferToEat = false;           // Prioritize the move towards the gas station
                bool preferToUnload = false;        // Prioritize the move towards the unloading zone
                double eatingPreferranceIfIsPreferToEat = 0.0;    // If gas station should be prioritized, calculate the probability of moving there
                double unloadPreferranceIfIsPreferToUnload = 0.0; // If Loading zone should be prioritized, calculate the probability of moving there
                double countOtherMovements = 0.0;
                double remainingProbability = 1.0;

                {
                    double tradeOff = 0.0;

                    if (std::floor(pairDistance(S.currentCellCoord, fillingStationCoordinate)) >= S.satiety) {
                        preferToEat = true;
                        eatingPreferranceIfIsPreferToEat = 1.0 - ((S.satiety)/(S.satiety+1.0));
                    }
                    if ((S.isLoadedOrEmpty != LoadType::FuelOrNoop)) {
                        preferToUnload = true;
                        unloadPreferranceIfIsPreferToUnload = std::floor(pairDistance(S.currentCellCoord, unloadingCoordinate));
                        unloadPreferranceIfIsPreferToUnload = (unloadPreferranceIfIsPreferToUnload/(unloadPreferranceIfIsPreferToUnload+1.0));
                    }
                    tradeOff = EatVsUnloadPreferrance * (eatingPreferranceIfIsPreferToEat) + (1.0-EatVsUnloadPreferrance) * unloadPreferranceIfIsPreferToUnload;
                    if (tradeOff > 0.0) {
                        eatingPreferranceIfIsPreferToEat = EatAndUnloadVsRest * EatVsUnloadPreferrance * (eatingPreferranceIfIsPreferToEat) / tradeOff;
                        unloadPreferranceIfIsPreferToUnload = EatAndUnloadVsRest * (1.0-EatVsUnloadPreferrance) * (unloadPreferranceIfIsPreferToUnload) / tradeOff;
                        remainingProbability -= eatingPreferranceIfIsPreferToEat;
                        remainingProbability -= unloadPreferranceIfIsPreferToUnload;
                    }
                }

                if ((S.satiety >= 2.1) &&
                    (S.currentCellCoord == unloadingCoordinate) &&
                        S.isExactAmount()/*(std::find(S.UnloadZoneContent.begin(), S.UnloadZoneContent.end(), LoadType::OneStone)) != S.UnloadZoneContent.end()*/) {
                    EnvironmentStatus result = S;
                    result.satiety -= 1.0;
                    result.nActionsPerformed++;
                    result.remaining_time--;
                    result.UnloadZoneContent.emplace_back(LoadType::FuelOrNoop);
                    if (result.isGameProgressPositive())
                        result.game_progress++;
                    SerializableRule rule{RuleCases::UnloadResource};
                    rule.feedback = countWeightDifference(S, result);
                    G.adjacency_graph[S][result].emplace_back(rule);
                    dstId = generateStateId(result);
                    if (debug) os << srcId << "{" << S << "}--[" << rule << "]-->" << dstId << "{" << result << "}" << std::endl<< std::endl;
                    DFSGeneratePossibleStates(os, G, result, S.currentCellCoord);
                    countOtherMovements++;
                }

                if ((S.satiety >= 0.1) &&
                    (S.currentCellCoord == unloadingCoordinate) &&
                    (S.isRightAmount())) {
                    EnvironmentStatus result = S;
                    result.isIgnited = true;
                    result.nActionsPerformed++;
                    result.satiety -= 0.1;
                    result.remaining_time--;
                    if (result.isGameProgressPositive())
                        result.game_progress++;
                    SerializableRule rule{RuleCases::Ignite};
                    rule.feedback = countWeightDifference(S, result);
                    G.adjacency_graph[S][result].emplace_back(rule);
                    dstId = generateStateId(result);
                    if (debug) os << srcId << "{" << S << "}--[" << rule << "]-->" << dstId << "{" << result << "}" << std::endl<< std::endl;
                    DFSGeneratePossibleStates(os, G, result, S.currentCellCoord);
                    countOtherMovements++;
                }

                if ((S.isLoadedOrEmpty != LoadType::FuelOrNoop) &&
                        (S.currentCellCoord == unloadingCoordinate) &&
                        (S.satiety >= 0.1)) {

                    EnvironmentStatus result = S;
                    result.UnloadZoneContent.emplace_back(S.isLoadedOrEmpty);
                    result.isLoadedOrEmpty = LoadType::FuelOrNoop;
                    result.remaining_time--;
                    result.nActionsPerformed++;
                    result.satiety -= 0.1;
                    if (result.isGameProgressPositive())
                        result.game_progress++;
                    SerializableRule rule{RuleCases::UnloadResource};
                    rule.feedback = countWeightDifference(S, result);
                    G.adjacency_graph[S][result].emplace_back(rule);
                    dstId = generateStateId(result);
                    if (debug) os << srcId << "{" << S << "}--[" << rule << "]-->" << dstId << "{" << result << "}" << std::endl<< std::endl;
                    DFSGeneratePossibleStates(os, G, result, S.currentCellCoord);
                    countOtherMovements++;
                }

                for (size_t i = 0, N = LogCellsPosition.size(); i<N; i++) {
                    const std::pair<size_t, size_t>& logCoord = LogCellsPosition.at(i);
                    if ((S.currentCellCoord == logCoord) && (S.LogCellsContent.at(i) > 0) && (S.isLoadedOrEmpty == LoadType::FuelOrNoop) && (S.satiety >= 0.1)) {

                        EnvironmentStatus result = S;
                        result.isLoadedOrEmpty = LoadType::OneLog;
                        result.remaining_time--;
                        result.nActionsPerformed++;
                        result.LogCellsContent[i]--;
                        result.satiety -= 0.1;
                        SerializableRule rule{RuleCases::LoadResource};
                        rule.feedback = countWeightDifference(S, result);
                        G.adjacency_graph[S][result].emplace_back(rule);
                        dstId = generateStateId(result);
                        if (debug) os << srcId << "{" << S << "}--[" << rule << "]-->" << dstId << "{" << result << "}" << std::endl<< std::endl;
                        DFSGeneratePossibleStates(os, G, result, S.currentCellCoord);
                        countOtherMovements++;
                    }
                }

                for (size_t i = 0, N = StoneCellsPosition.size(); i<N; i++) {
                    const std::pair<size_t, size_t>& stoneCoord = StoneCellsPosition.at(i);
                    if ((S.currentCellCoord == stoneCoord) && (S.StoneCellsContent.at(i) > 0) && (S.isLoadedOrEmpty == LoadType::FuelOrNoop) && (S.satiety >= 0.1)) {

                        EnvironmentStatus result = S;
                        result.isLoadedOrEmpty = LoadType::OneStone;
                        result.remaining_time--;
                        result.nActionsPerformed++;
                        result.StoneCellsContent[i]--;
                        result.satiety -= 0.1;
                        SerializableRule rule{RuleCases::LoadResource};
                        rule.feedback = countWeightDifference(S, result);
                        G.adjacency_graph[S][result].emplace_back(rule);
                        dstId = generateStateId(result);
                        if (debug) os << srcId << "{" << S << "}--[" << rule << "]-->" << dstId << "{" << result << "}" << std::endl<< std::endl;
                        DFSGeneratePossibleStates(os, G, result, S.currentCellCoord);
                        countOtherMovements++;
                    }
                }

                if ((S.currentCellCoord == fillingStationCoordinate) && (S.satiety < maxSatiety) && (S.satiety > 0.0)) {
                    EnvironmentStatus result = S;
                    result.remaining_time--;
                    result.nActionsPerformed++;
                    result.satiety = std::min(maxSatiety, result.satiety+5.0);
                    SerializableRule rule{RuleCases::GainEnergy};
                    rule.feedback = countWeightDifference(S, result);
                    G.adjacency_graph[S][result].emplace_back(rule);
                    dstId = generateStateId(result);
                    if (debug) os << srcId << "{" << S << "}--[" << rule << "]-->" << dstId << "{" << result << "}"<< std::endl<< std::endl;
                    DFSGeneratePossibleStates(os, G, result, S.currentCellCoord);
                    countOtherMovements++;
                }

                std::vector<std::pair<Directions, std::pair<size_t, size_t>>> allowedDirections = generateDirections(S.currentCellCoord, boardSize.first, boardSize.second);
                allowedDirections.erase(std::remove_if(allowedDirections.begin(), allowedDirections.end(), [prevCell](const auto& x) { return x.second == prevCell; } ), allowedDirections.end());
                Directions priorityEatingCell, priorityUnloadCell;
                size_t hasAtLeastOnePreferredMovement = 0;
                if (preferToEat && (!allowedDirections.empty())) {
                    priorityEatingCell = rankDirections(allowedDirections, fillingStationCoordinate).begin()->first;
                }
                if (preferToUnload && (!allowedDirections.empty())) {
                    priorityUnloadCell = rankDirections(allowedDirections, unloadingCoordinate).begin()->first;
                }
                if (preferToEat && preferToUnload) {
                    if (priorityEatingCell == priorityUnloadCell)
                        hasAtLeastOnePreferredMovement = 1;
                    else
                        hasAtLeastOnePreferredMovement = 2;
                } else if (preferToEat || preferToUnload) {
                    hasAtLeastOnePreferredMovement = 1;
                } else {
                    hasAtLeastOnePreferredMovement = 0;
                }

                // Increase the cost of moving if the robot is loaded
                double costNormalPace = 1.0;
                double costFastPace = 2.0;
                if (S.isLoadedOrEmpty != LoadType::FuelOrNoop) {
                    costNormalPace += 0.5;
                    costFastPace += 1.0;
                }


                bool doFastPace = false;(!allowedDirections.empty()) && (S.satiety >= costFastPace);
                bool hasAnyMove = (!allowedDirections.empty()) && (S.satiety >= costNormalPace);

                if (!hasAnyMove) {
                    ///os << "Failing state is reached! " << srcId << std::endl;
                    G.failing_states.insert(S);
                } else {
                    double equiprobableProbabilitySum = 1.0;
                    ///double equiprobableProbability = 0.0;
                    size_t totalUnprioritizedMovements = allowedDirections.size() - hasAtLeastOnePreferredMovement;
                    if (preferToEat || preferToUnload) {
                        equiprobableProbabilitySum = remainingProbability;
                    } else {
                        equiprobableProbabilitySum = 1.0;
                    }
                    if (doFastPace) {
                        totalUnprioritizedMovements *= 2;
                        eatingPreferranceIfIsPreferToEat *= 0.5;
                        unloadPreferranceIfIsPreferToUnload *= 0.5;
                    }
                    countOtherMovements += totalUnprioritizedMovements;


                    double equiprobableProbability;
                    if (countOtherMovements > 0) {
                        equiprobableProbability = equiprobableProbabilitySum / (countOtherMovements);
                        for (auto& cp : G.adjacency_graph[S]) {
                            for (SerializableRule& item : cp.second) {
                                item.probability = equiprobableProbability;
                            }
                        }
                    } else {
                        eatingPreferranceIfIsPreferToEat = eatingPreferranceIfIsPreferToEat / EatAndUnloadVsRest;
                        unloadPreferranceIfIsPreferToUnload = unloadPreferranceIfIsPreferToUnload / EatAndUnloadVsRest;
                    }


                    // If the robot can use the energy in itself, then he can move!
                    if (hasAnyMove) {
                        // Normal pace movement

                        for (const std::pair<Directions, std::pair<size_t, size_t>>& cell : allowedDirections) {
                            EnvironmentStatus result = S;
                            result.remaining_time--;
                            result.satiety -= costNormalPace;
                            result.currentCellCoord = cell.second;
                            result.nActionsPerformed++;
                            SerializableRule rule{RuleCases::Move, cell.first, false};
                            rule.feedback = countWeightDifference(S, result);
                            bool cellEating = (cell.first == priorityEatingCell);
                            bool cellUnload = (cell.first == priorityUnloadCell);
                            if (preferToEat || preferToUnload) {
                                rule.probability = 0;
                                if (cellEating)
                                    rule.probability += eatingPreferranceIfIsPreferToEat;
                                if (cellUnload)
                                    rule.probability += unloadPreferranceIfIsPreferToUnload;
                                if ((!cellEating) && (!cellUnload)) {
                                    assert(countOtherMovements > 0);
                                    rule.probability = equiprobableProbability;
                                }
                            } else {
                                assert(countOtherMovements > 0);
                                rule.probability = equiprobableProbability;
                            }
                            G.adjacency_graph[S][result].emplace_back(rule);
                            dstId = generateStateId(result);
                            if (debug) os << srcId << "{" << S << "}--[" << rule << "]-->" << dstId << "{" << result << "}" << std::endl<< std::endl;
                            DFSGeneratePossibleStates(os, G, result, S.currentCellCoord);
                        }

                        if (doFastPace) {
                            for (const std::pair<Directions, std::pair<size_t, size_t>>& cell : allowedDirections) {
                                EnvironmentStatus result = S;
                                result.remaining_time-=0.5;
                                result.satiety -= costFastPace;
                                result.currentCellCoord = cell.second;
                                SerializableRule rule{RuleCases::FastMove, cell.first, true};
                                rule.feedback = countWeightDifference(S, result);
                                bool cellEating = (cell.first == priorityEatingCell);
                                bool cellUnload = (cell.first == priorityUnloadCell);
                                if (preferToEat || preferToUnload) {
                                    rule.probability = 0;
                                    if (cellEating)
                                        rule.probability += eatingPreferranceIfIsPreferToEat;
                                    if (cellUnload)
                                        rule.probability += unloadPreferranceIfIsPreferToUnload;
                                    if ((!cellEating) && (!cellUnload))
                                        rule.probability = equiprobableProbability;
                                } else {
                                    rule.probability = equiprobableProbability;
                                }
                                G.adjacency_graph[S][result].emplace_back(rule);
                                dstId = generateStateId(result);
                                os << srcId << "{" << S << "}--[" << rule << "]-->" << dstId << "{" << result << "}" << std::endl<< std::endl;
                                DFSGeneratePossibleStates(os, G, result, S.currentCellCoord);
                            }
                        }
                    }

                    double testProbability = 0.0;
                    for (auto& cp : G.adjacency_graph[S]) {
                        for (SerializableRule& item : cp.second) {
                            testProbability += item.probability;
                        }
                    }
                    if (std::abs(testProbability - 1.0) > std::numeric_limits<double>::epsilon())
                        assert(std::abs(testProbability - 1.0) <= 0.000000000000001);

                    ///os.flush();
                }

                ///os.flush();

            }

            ///os.flush();
            ///std::cout << std::endl;
        }
    }

};

#include <fstream>

int main(void) {
    Board gameBoard{3, 3,
                    2, 1,
                    2, 2,
                    0, 0,
                    11,   // 10: 107582 stati, nessuno di accettazione
                    50};
    gameBoard.addLogCell(2, 0, 4);
    //gameBoard.addLogCell(3, 11, 2);
    //gameBoard.addLogCell(10, 12, 1);
    gameBoard.addStoneCell(0, 2, 6);
    //gameBoard.addStoneCell(3, 5, 1);
    //gameBoard.addStoneCell(9, 10, 3);

    std::ofstream f{"testing.txt"};
    auto g = gameBoard.generatePossibleStates(f);
    std::cout << "Total States: " << g.adjacency_graph.size() << std::endl;
    std::cout << " - Winning States: " << ((double)g.accepting_states.size())/((double)g.adjacency_graph.size()) << std::endl;
    std::cout << " - Losing States: " << ((double)g.failing_states.size())/((double)g.adjacency_graph.size()) << std::endl;
    std::cout << " - Final States: " << ((double)(g.accepting_states.size()+g.failing_states.size()))/((double)g.adjacency_graph.size()) << std::endl;
}