import sys
import time

#while True:
#time.sleep(1)
print("OK, START")
sys.stdout.flush()

while True:
    time.sleep(1)
    #sys.stdout.write("READY\n")
    line = sys.stdin.readline().strip()
    f=file("stdin.txt",mode='a')
    if len(line) > 0:
        if line.upper() == "QUIT":
            print("OK, QUIT")
            exit(0)
        try:
            print("OK, "+str(float(line)*2))
            #sys.stdout.write("OK, "+str(float(line)*2)+"\n")
            f.write("ok ")
        except:
            print("Parse error, "+line)
            f.write("error ")
        sys.stderr.write('spam\n')

        f.write(line+'\n')

        sys.stdout.flush()


    f.close()



