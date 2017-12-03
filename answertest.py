import sys
import time


def StdOut(msg):
	sys.stdout.write(msg + "\n")
	sys.stdout.flush()  # some languages need manual flushing to work - python is one of them


def StdErr(msg):
	sys.stderr.write(msg + "\n")
	sys.stderr.flush()


while True:  # loop forever to keep app loaded

	# tell unity we're ready to receive input
	StdOut("READY")

	line = sys.stdin.readline().strip()

	if len(line) > 0:
		#f=file("stdin.txt",mode='a') # for debugging
		try:
			processedValues = float(line) * 2  # some really complicated maths
			time.sleep(1)  # simulate procesing time

			StdOut(str(processedValues))
			#f.write("ok ")
		except:
			StdErr("Parse error, " + line)
			#f.write("error ")
		#f.write(line+'\n')
		#f.close()
