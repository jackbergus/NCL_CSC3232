import numpy as np
import matplotlib.pyplot as plt

ladders = [(3,19), (15,37), (22,42), (25,64), (41,73),
           (53,74), (63,86), (76,91), (84,98)]
snakes = [(11,7), (18,13), (28,12), (36,34), (77,16),
          (47,26), (83,39), (92,75), (99,70)]
trans = ladders + snakes

# Set up the transition matrix
T = np.zeros((101, 101))
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

fig, ax = plt.subplots()
ax.matshow(T, cmap=plt.cm.Blues)
fig.savefig('second_matrix.png')
