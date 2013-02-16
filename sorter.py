import serial, time

def start(port, baud=9600):
    ser = serial.Serial(port, baud, timeout=1)

def getBall(ser):
    ser.write(chr(1))
    ser.write(chr(0))

def releaseBall(ser):
    ser.write(chr(3))
    ser.write(chr(2))

def index(ser, n):
    for ball in range(n):
        getBall(ser)
        time.sleep(2)
        releaseBall(ser)
