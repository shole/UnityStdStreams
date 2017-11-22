
import sys

for line in sys.stdin:
    if (line=="QUIT"):
        exit()
    print(float(line)*2)
