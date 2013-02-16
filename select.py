import serial, time

def start(port, baud=9600):
    ser = serial.Serial(port, baud, timeout=1)

def run(port, baud=9600, n, t):
    ser = serial.Serial(port, baud, timeout=1)
    index(ser, n)
    ser.close()

def getBall(ser):
    ser.write(chr(1))
    time.sleep(1)
    ser.write(chr(0))

def releaseBall(ser):
    ser.write(chr(3))
    time.sleep(1)
    ser.write(chr(2))

def index(ser, n, t=2):
    for ball in range(n):
        getBall(ser)
        time.sleep(t)
        releaseBall(ser)
