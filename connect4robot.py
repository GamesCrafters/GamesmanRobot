import urllib
import string
#import serial

class RobotMover:
    def __init__(self, port = '/dev/ttyACM0', rate = 9600):
        self.robotSocket = serial.Serial(port, rate)

    def move(self, column):
        self.robotSocket.write(bytes([column + 48])) # '0' == 48 (ASCII)
        return int(self.robotSocket.read())

BASEURL = "http://nyc.cs.berkeley.edu:8080/gcweb/service/gamesman/puzzles/connect4/getNextMoveValues;width=7;height=6;pieces=4;board="
BOARD   = "                                          "
DONEBOARD = "XXXOOOO                                   "
MEMOIZED_TABLE = {}

#returns true if board is primitive (game over)
def primitive(board):
    return (board_to_response(board) == [])

#returns a winning move with lowest remoteness
#or a tie move with highest remoteness
#or a losing move with highest remoteness
def best_move(moves):
    low_remote_win = {'remoteness': 9001}
    high_remote_tie = {'remoteness': -1}
    high_remote_lose = {'remoteness': -1}
    for move in moves:
        if move['value'] == 'win':
            if move['remoteness'] < low_remote_win['remoteness']:
                low_remote_win = move
        elif move['value'] == 'tie':
            if move['remoteness'] > high_remote_tie['remoteness']:
                high_remote_tie = move
        elif move['value'] == 'lose':
            if move['remoteness'] > high_remote_lose['remoteness']:
                high_remote_lose = move
        else:
            print 'best_move: move[\'value\'] returns neither a \'win\' or \'lose\''
    if low_remote_win['remoteness'] < len(BOARD)+1:
        return low_remote_win
    elif high_remote_tie['remoteness'] > -1:
        return high_remote_tie
    elif high_remote_lose['remoteness'] > -1:
        return high_remote_lose
    else:
        print 'best_move: not returning a valid move'


#takes in a string board representation
#returns a list of possible moves
#moves are represented as dictionaries with the following keys:
#move(int), board(string), value('win' or 'lose'), remoteness (int)
def board_to_response(board):
    board = string.replace(board, " ", "%20")
    global MEMOIZED_TABLE
    if board in MEMOIZED_TABLE:
        return MEMOIZED_TABLE[board]
    else:
        url = urllib.urlopen(BASEURL + board)
        html = url.read()
        url.close()
        ans = eval(html)['response']
        MEMOIZED_TABLE[board] = ans
        return ans

#prints out an ascii representation of the board
def print_board(board):
    for i in range(5,-1,-1):
        lineSoFar = ""
        for j in range(0,7):
            lineSoFar += "|"
            lineSoFar += board[i*7 + j]
        print "|" + lineSoFar + "||"

#plays a game with itself, useful
def play_game_robotVrobot(board):
    #robot = RobotMover()
    while not primitive(board):
        #board = update_board()
        #print board
        moves = board_to_response(board)
        nextMove = best_move(moves)
        print nextMove
        #robot.move(nextMove['move'])
        board = nextMove['board']
        print_board(board)


    


#play_game_robotVRobot(BOARD);
play_game(BOARD,("r","r"))
