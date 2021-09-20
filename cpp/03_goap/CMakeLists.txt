cmake_minimum_required(VERSION 3.18)
project(goap)

set(CMAKE_CXX_STANDARD 20)

add_executable(goap main.cpp)
target_link_libraries(goap yaucl_hashing)

add_executable(robot robot.cpp)
target_link_libraries(robot yaucl_hashing)

add_executable(study_party study_party.cpp)
target_link_libraries(study_party yaucl_hashing)