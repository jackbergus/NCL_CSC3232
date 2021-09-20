import numpy as np
import matplotlib.pyplot as plt

ladders = []#(3,19), (15,37), (22,42), (25,64), (41,73), (53,74), (63,86), (76,91), (84,98)
snakes = [(11,7), (18,13), (28,12), (36,34), (77,16),
          (47,26), (83,39), (92,75), (99,70)]
trans = ladders + snakes

# Set up the transition matrix
T = np.zeros((101, 101))
A = np.zeros((101, 101))
b = np.zeros(101)
for i in range(1,101):
    T[i-1,i:i+6] = 1/6

for (i1,i2) in trans:
    iw = np.where(T[:,i1] > 0)
    T[:,i1] = 0
    T[iw,i2] += 1/6

# House rules: you don't need to land on 100, just reach it.
T[95:100,100] += np.linspace(1/6, 5/6, 5)
for snake in snakes:
    T[snake,100] = 0


for i in range(0,101):
   if (i == 100):
      b[i] = 0
   else:
      b[i] = -1
   for j in range(0,101):
     if (i == j):
       if (i == 100):
          A[i,j] = 1
       else:
          A[i,j] = -1
     elif (i == 100):
       A[i,j] = 0
     else:
       A[i,j] = T[i,j]


print(np.linalg.solve(A,b)[0])

# The player starts at position 0.
v = np.zeros(101)
v[0] = 1

minn = 999999999999999999
maxn = -1
maxprob = -1
n, P = 0, []
cumulative_prob = 0
# Update the state vector v until the cumulative probability of winning
# is "effectively" 1
while cumulative_prob < 0.99999:
    n += 1
    v = v.dot(T)
    if (v[100]) >0:
        maxn = max(maxn, n)
        minn = min(minn, n)
    if (v[100] > val):
        val = v[100]
        maxprob = n
    P.append(v[100])
    cumulative_prob += P[-1]

print(minn)
print(maxn)
print(str(maxprob)+" with prob="+str(val))
mode = np.argmax(P)+1
print('modal number of moves:', mode)

# Plot the probability of winning as a function of the number of moves
fig, ax = plt.subplots()
ax.plot(np.linspace(1,n,n), P, 'g-', lw=2, alpha=0.6, label='Markov')
ax.set_xlabel('Number of moves')
ax.set_ylabel('Probability of winning')

fig.savefig('Prob_winning_S.png')
