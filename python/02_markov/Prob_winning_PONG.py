#!/usr/bin/env python3
import numpy as np
import matplotlib.pyplot as plt
import csv

# Set up the transition matrix

A = np.zeros((5, 5))
b = np.zeros(5)

def init_T(p,q):
   T = np.zeros((5, 5))
   T[0,1] = 0.5
   T[0,2] = 0.5
   T[1,2] = p
   T[1,4] = 1-p
   T[2,1] = q
   T[2,3] = 1-q
   return T

final_states = {3,4}
initial_state = 0
probabilities = [0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9]

for i in range(0,5):
   if (i in final_states):
      b[i] = 0
   else:
      b[i] = -1

with open('lin_al_solve_PONG.csv','w') as f:
 writer = csv.writer(f)
 writer.writerow(['p','q','mean'])
 
 p1f = open('p1_winning.csv','w')
 p1w = csv.writer(p1f)
 p1w.writerow(['p','q','moves','prob'])
 
 p2f = open('p2_winning.csv','w')
 p2w = csv.writer(p2f)
 p2w.writerow(['p','q','moves','prob'])
 for p in probabilities:
  for q in probabilities:
     T = init_T(p, q)
     for i in range(0,5):
        for j in range(0,5):
          if (i == j):
            if (i in final_states):
               A[i,j] = 1
            else:
               A[i,j] = -1
          elif (i in final_states):
            A[i,j] = 0
          else:
            A[i,j] = T[i,j]
     writer.writerow([p, q, np.linalg.solve(A,b)[initial_state]])
 
     
     # The player starts at position 0.
     v = np.zeros(5)
     v[0] = 1
     n= 0
     cumulative_prob1 = 0
     cumulative_prob2 = 0
     # Update the state vector v until the cumulative probability of winning
     # is "effectively" 1
     while ((cumulative_prob1 < 0.99999) and (cumulative_prob2 < 0.99999)) and ((n<100) or ((abs(v[4]) >= 1e-16) and abs(v[3]) >= 1e-16)):
      n += 1
      v = v.dot(T)
      p1w.writerow([p,q,n,v[3]])
      p2w.writerow([p,q,n,v[4]])
      cumulative_prob1 += v[3]
      cumulative_prob2 += v[4]
 
 p1f.close()
 p2f.close()
