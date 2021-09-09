/*
 * main.cpp
 * This file is part of probability/cpp
 *
 * Copyright (C) 2021 - Giacomo Bergami
 *
 * probability/cpp is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * probability/cpp is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with probability/cpp. If not, see <http://www.gnu.org/licenses/>.
 */

 

#include <vector>
#include <string>

struct door {
    bool bricked;
    bool wrong_door;
    bool is_choosen;
    ssize_t reachable_state;

    door() : door(true, true, -1) {}
    door(const door& ) = default;
    door(door&& ) = default;
    door& operator=(const door& ) = default;
    door& operator=(door&& ) = default;
    door(bool bricked, bool wrongDoor, size_t reachableState) : bricked(bricked), wrong_door(wrongDoor),
    reachable_state(reachableState), is_choosen{false} {}
};

#include <cmath>

struct board {
    std::vector<std::vector<door>> transitions_from_states;
    std::vector<bool> state_navigation;
    size_t total_states;
    size_t start_state;
    size_t end_state;

    board(size_t n, size_t doors) : total_states{n}, start_state{0}, end_state{(n == 0 ? 0 : n-1)} {
        for (size_t i = 0; i<n; i++)
                transitions_from_states.emplace_back((i != end_state) ? doors : 0);
    }

    board() : board(4, 6) {}
    board(const board& ) = default;
    board(board&& ) = default;
    board& operator=(const board& ) = default;
    board& operator=(board&& ) = default;
};

void first_scenario(board& board) {
    size_t i = 0;
    for (std::vector<door>& adj : board.transitions_from_states) {
        if (!adj.empty()) {
            auto ptr = adj.rbegin();
            ptr->bricked = false;
            ptr->wrong_door = false;
            ptr->reachable_state = (ssize_t)++i;
        }
    }
}

void second_scenario(board& board) {
    size_t i = 1;
    for (std::vector<door>& adj : board.transitions_from_states) {
        for (size_t j = 0, N = adj.size(); j<N; j++) {
            auto& ref = adj[j];
            ref.bricked = false;
            ref.reachable_state = (ssize_t)i;
            if (j == (N-1)) {
                ref.wrong_door = false;
            }
        }
        i++;
    }
}

#include <functional>
#include <iostream>

bool current_door_step(board& board,
                       size_t& curr_state,
                       door& d,
                       size_t& door_id,
                       size_t& attempts,
                       bool set_choosen,
                       std::function<bool(size_t&, struct board&, struct door&)> f) {

    if (set_choosen) {
        // Never choose a previously choosen door: continue with the iteration
        if (d.is_choosen) return false;
        std::cout << "   * Current Door = " << door_id << std::endl;
        d.is_choosen = true;
    }

    attempts++;
    if (d.bricked) {
        std::cout << "   Door is bricked!" << std::endl;
    } else if (d.wrong_door) {
        std::cout << "   Wrong door!" << std::endl;
        return f(curr_state, board, d);
    } else {
        std::cout << "   Moving towards state #" << (size_t)d.reachable_state << std::endl;
        curr_state = (size_t)d.reachable_state;
        board.state_navigation.emplace_back(true);
        return true; // Killing the iteration!
    }
    return false; // Do not kill the iteration
}

void navigate(board& board,
              bool worst_case_scenario,
              bool set_choosen,
              std::function<bool(size_t&, struct board&, struct door&)> on_wrong_door_do,
              std::function<void(size_t&, struct board&)> on_wrong_state_do) {
    size_t attempts = 0;
    size_t curr_state = board.start_state;
    if (worst_case_scenario)
        std::cout << "Simulating the Worst Case Scenario" << std::endl;
    else
        std::cout << "Simulating the Best Case Scenario" << std::endl;
    while (curr_state != board.end_state) {
        std::cout << " * Current State = " << curr_state << std::endl;
        size_t i = 0;
        if (worst_case_scenario) {
            i = 1;
            auto en = board.transitions_from_states[curr_state].end();
            for (auto it = board.transitions_from_states[curr_state].begin(); it != en; it++) {
                if (current_door_step(board, curr_state, *it, i, attempts, set_choosen, on_wrong_door_do)) break;
                i++;
            }
        } else {
            i = board.transitions_from_states[curr_state].size();
            auto en = board.transitions_from_states[curr_state].rend();
            for (auto it = board.transitions_from_states[curr_state].rbegin(); it != en; it++) {
                if (current_door_step(board, curr_state, *it, i, attempts, set_choosen, on_wrong_door_do)) break;
                i--;
            }
        }
        if (curr_state == board.end_state) {
            bool is_ok = true;
            for (bool v : board.state_navigation) {
                if (!v) {
                    is_ok = false;
                    break;
                }
            }
            if (!is_ok)
                on_wrong_state_do(curr_state, board);
        }
    }
    std::cout << "Total attempts = " << attempts << std::endl;
}

void first_run() {
    auto on_wrong_do_nothing = [](size_t&, struct board&, struct door&) {return true;};
    auto all_final_states_are_ok = [](size_t&, struct board&) {};

    {
        board default_board;
        first_scenario(default_board);
        navigate(default_board, true, true, on_wrong_do_nothing, all_final_states_are_ok);
    }
    {
        board default_board;
        first_scenario(default_board);
        navigate(default_board, false, true, on_wrong_do_nothing, all_final_states_are_ok);
    }
}

void second_run(bool set_choosen = false) {
    auto on_wrong_do_ignore_backtrack_later = [](size_t& curr_state, struct board& board, struct door& d) {
        std::cout << "   Moving towards state #" << ((size_t)d.reachable_state)<< std::endl;
        curr_state = (size_t)d.reachable_state;
        board.state_navigation.emplace_back(false);
        return true;
    };
    auto on_wrong_final_state_backtrack = [](size_t& curr_state, struct board& board) {
        curr_state = board.start_state;
        board.state_navigation.clear();
    };

    {
        board default_board;
        second_scenario(default_board);
        navigate(default_board, true, set_choosen, on_wrong_do_ignore_backtrack_later, on_wrong_final_state_backtrack);
    }

    {
        board default_board;
        second_scenario(default_board);
        navigate(default_board, false, set_choosen, on_wrong_do_ignore_backtrack_later, on_wrong_final_state_backtrack);
    }
}

#include <yaucl/hashing/hash_combine.h>
#include <yaucl/hashing/pair_hash.h>
#include <unordered_set>
#include <unordered_map>
#include <array>

void third_run_sequential() {

    board default_board;
    second_scenario(default_board);

    // Remembering the wrong configurations
    std::vector<std::unordered_set<std::pair<size_t, size_t>>>                s1_position_to_other_choices{6};
    std::vector<std::unordered_set<std::pair<size_t, size_t>>>                s2_position_to_other_choices{6};
    std::vector<std::unordered_set<std::pair<size_t, size_t>>>                s3_position_to_other_choices{6};
    std::unordered_map<std::pair<size_t, size_t>, std::unordered_set<size_t>> s1_s2_position_to_other_choices;
    std::unordered_map<std::pair<size_t, size_t>, std::unordered_set<size_t>> s1_s3_position_to_other_choices;
    std::unordered_map<std::pair<size_t, size_t>, std::unordered_set<size_t>> s2_s3_position_to_other_choices;
    std::unordered_set<std::pair<size_t, std::pair<size_t, size_t>>>          s1_s2_s3_attempts; // Not necessary here!
    // Counting the attempts
    size_t attempts = 0;

    for (size_t i = 0; i<6; i++) {
        if (s1_position_to_other_choices.at(i).size() == 36)  {
            std::cout << "Skipping door@s=" << i << std::endl;
            continue;
        }

        for (size_t j = 0; j<6; j++) {
            if (s2_position_to_other_choices.at(j).size() == 36) {
                std::cout << "Skipping door@s1=" << j << std::endl;
                continue;
            }
            {
                auto it = s1_s2_position_to_other_choices.find({i,j});
                if (it != s1_s2_position_to_other_choices.end() && it->second.size() == 6) {
                    std::cout << "Skipping door@s=" << i << " and door@s1=" << j << std::endl;
                    continue;
                }
            }

            for (size_t k = 0; k<6; k++) {
                if (s3_position_to_other_choices.at(k).size() == 36) continue;
                {
                    auto it = s1_s3_position_to_other_choices.find({i,k});
                    if (it != s1_s3_position_to_other_choices.end() && it->second.size() == 6) {
                        std::cout << "Skipping door@s=" << i << " and door@s2=" << k << std::endl;
                        continue;
                    }
                }
                {
                    auto it = s2_s3_position_to_other_choices.find({j,k});
                    if (it != s2_s3_position_to_other_choices.end() && it->second.size() == 6) {
                        std::cout << "Skipping door@s1=" << j << " and door@s2=" << k << std::endl;
                        continue;
                    }
                }
                if (s1_s2_s3_attempts.contains({i, {j, k}})) {
                    std::cout << "Skipping " << i << ' ' << j << ' ' << k << "!" << std::endl;
                    continue;
                }

                if (default_board.transitions_from_states.at(0).at(i).wrong_door ||
                    default_board.transitions_from_states.at(1).at(j).wrong_door ||
                    default_board.transitions_from_states.at(2).at(k).wrong_door) {
                    std::cout << "Wrong configuration: " << i << ' ' << j << ' ' << k << "!" << std::endl;
                    s1_position_to_other_choices[i].insert({j,k});
                    s2_position_to_other_choices[j].insert({i,k});
                    s3_position_to_other_choices[k].insert({i,j});
                    s1_s2_position_to_other_choices[{i,j}].insert(k);
                    s1_s3_position_to_other_choices[{i,k}].insert(j);
                    s2_s3_position_to_other_choices[{j,k}].insert(i);
                    s1_s2_s3_attempts.insert({i, {j,k}});
                    attempts++;
                } else {
                    std::cout << "Winning configuration: " << i << ' ' << j << ' ' << k << '!' << std::endl;
                }
            }
        }
    }

    std::cout << "attempts: " << (attempts+1) << std::endl;
}

#include <random>
#include <cassert>

size_t third_run_random(size_t generator_seed, bool debug = true) {

    board default_board;
    second_scenario(default_board);

    std::random_device  dev;
    std::mt19937_64     generator_s1(generator_seed);
    std::mt19937_64     generator_s2(generator_seed+1);
    std::mt19937_64     generator_s3(generator_seed+2);
    std::uniform_int_distribution<size_t> door_distr(0,5);

    // Remembering the wrong configurations
    std::vector<std::unordered_set<std::pair<size_t, size_t>>>                s1_position_to_other_choices{6};
    std::vector<std::unordered_set<std::pair<size_t, size_t>>>                s2_position_to_other_choices{6};
    std::vector<std::unordered_set<std::pair<size_t, size_t>>>                s3_position_to_other_choices{6};
    std::unordered_map<std::pair<size_t, size_t>, std::unordered_set<size_t>> s1_s2_position_to_other_choices;
    std::unordered_map<std::pair<size_t, size_t>, std::unordered_set<size_t>> s1_s3_position_to_other_choices;
    std::unordered_map<std::pair<size_t, size_t>, std::unordered_set<size_t>> s2_s3_position_to_other_choices;
    std::unordered_set<std::pair<size_t, std::pair<size_t, size_t>>>          s1_s2_s3_attempts;
    // Counting the attempts
    size_t attempts = 0;

    bool not_found = true;
    while (not_found) {
        size_t i, j, k;
        size_t tryouts = 0;
        do {
            tryouts++;
            if (debug && (tryouts > 1)) std::cout << "changing i" << std::endl;
            i = door_distr(generator_s1);
        } while (s1_position_to_other_choices.at(i).size() == 36);

        tryouts = 0;
        do {
            tryouts++;
            if (debug && (tryouts > 1))  std::cout << "changing j" << std::endl;
            j = door_distr(generator_s2);
        } while ((s2_position_to_other_choices.at(j).size() == 36) ||
                 (s1_s2_position_to_other_choices[{i,j}].size() == 6));

        tryouts = 0;
        do {
            tryouts++;
            if (debug && (tryouts > 1))  std::cout << "changing k" << std::endl;
            k = door_distr(generator_s3);
        } while ((s1_s2_s3_attempts.contains({i, {j, k}})) ||
                 (s3_position_to_other_choices.at(k).size() == 36) ||
                 (s1_s3_position_to_other_choices[{i,k}].size() == 6) ||
                 (s2_s3_position_to_other_choices[{j,k}].size() == 6));

        if (default_board.transitions_from_states.at(0).at(i).wrong_door ||
            default_board.transitions_from_states.at(1).at(j).wrong_door ||
            default_board.transitions_from_states.at(2).at(k).wrong_door) {
            if (debug) std::cout << "Wrong configuration: " << i << ' ' << j << ' ' << k << "!" << std::endl;
            s1_position_to_other_choices[i].insert({j,k});
            s2_position_to_other_choices[j].insert({i,k});
            s3_position_to_other_choices[k].insert({i,j});
            s1_s2_position_to_other_choices[{i,j}].insert(k);
            s1_s3_position_to_other_choices[{i,k}].insert(j);
            s2_s3_position_to_other_choices[{j,k}].insert(i);
            s1_s2_s3_attempts.insert({i, {j,k}});
            attempts++;
        } else {
            if (debug) std::cout << "Winning configuration: " << i << ' ' << j << ' ' << k << '!' << std::endl;
            not_found = false;
        }
    }

    if (debug) std::cout << "attempts: " << (attempts+1) << std::endl<< std::endl<< std::endl;
    assert(attempts <= 216);
    return (attempts+1);
}

int main(void) {
    double sum = 0;
    double max = 10000;
    ssize_t max_val = -1;
    size_t min_val = std::numeric_limits<size_t>::max();
    for (size_t i = 0; i<(size_t)max; i++) {
        size_t val = third_run_random(i, false);
        min_val = std::min(val, min_val);
        max_val = std::max((ssize_t)val, max_val);
        sum += (double)val;
    }
    std::cout << "MIN = " << min_val << std::endl;
    std::cout << "MAX = " << max_val << std::endl;
    std::cout << "Average = " << (sum/max) << std::endl;
}
