rng = list(range(9,0,-1))
rrn = list(range(1,10))

def extend_at(f):
	l = list(map(lambda x : (x,0+f), rng))
	l.append((1,1+f))
	for x in rrn:
		l.append((x,2+f))
	l.append((9,3+f))
	return l
	
board = list()
board.extend(extend_at(0))
board.extend(extend_at(4))
board.extend(extend_at(8))
board.extend(extend_at(12))
board.extend(extend_at(16))
board[99] = (10,18)
