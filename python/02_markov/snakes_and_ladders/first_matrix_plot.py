import numpy as np
import matplotlib.pyplot as plt

# Set up the transition matrix
T = np.zeros((101, 101))
for i in range(1,101):
    T[i-1,i:i+6] = 1/6

# House rules: you don't need to land on 100, just reach it.
T[95:100,100] += np.linspace(1/6, 5/6, 5)

fig, ax = plt.subplots()
ax.matshow(T, cmap=plt.cm.Blues)
fig.savefig('first_matrix.png')
