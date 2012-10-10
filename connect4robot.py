import urllib
import string
#import serial

class RobotMover:
    def __init__(self, port = '/dev/ttyACM0', rate = 9600):
        self.robotSocket = serial.Serial(port, rate,timeout = 1)

    """
    send the column number to robot
    todo:  implement proper handshaking
    """
    def move(self, column):  
        self.robotSocket.write(bytes([column + 48])) # '0' == 48 (ASCII)
        return int(self.robotSocket.read())

    """
    reads information from the robot
    todo:  figure out the protocols here
    """
    def read(self):
        return int(self.robotSocket.read())  #

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

##allows the user to pick which move to make
def player_pick_move(moves):
    availableOptions = {}  #will contain all possible moves for the current state (Handles full columns)
    properMoves = ["1","2","3","4","5","6","7"]  #available moves
    for move in moves:
        availableOptions[int(move['move'])] = move
    choice = raw_input("Enter your move (0:7): ")  #gets input
    if(len(choice) > 1 or len(choice) == 0 or not choice in properMoves):  #checks if input is a number 1-7
        print "Bad choice.  Enter number 1-7."
        return player_pick_move(moves)
    choice = int(choice) - 1  #casts to int
    try:
        return availableOptions[choice]  #tries to get the move value of our choice, if it fails...
    except:
        print "Bad choice.  Enter number 1-7 that isn't a full column."
        return player_pick_move(moves)  #print error message, try again
    

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
def print_board(moves,choice):
    board = choice['board']
    moveValues = ['B','B','B','B','B','B','B']
    for item in moves:
        if item['value'] == 'win':
            moveValues[int(item['move'])] = 'W'
        if item['value'] == 'tie':
            moveValues[int(item['move'])] = 'T'
        if item['value'] == 'lose':
            moveValues[int(item['move'])] = 'L'
            
    lineSoFar = "||"
    for item in moveValues:
        lineSoFar += item + "|"
    print lineSoFar + "|"
        
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

#plays a game, mode[0] versus mode[1] (passed in as a tuple of (player1,player2), domain {"robot","player"}
def play_game(board,mode):
    print "START BOARD"
    print_board(board_to_response(BOARD),{"board":BOARD})
    
    currentPlayer = 0
    while not primitive(board):
        if(mode[currentPlayer] == "player"):  #play a single round as a player
            print "******Player's turn.*******"
            moves = board_to_response(board)
            nextMove = player_pick_move(moves)
            board = nextMove['board']
        else:  #play a single round as robot, the default
            print "*******Robot's turn.*******"
            moves = board_to_response(board)
            nextMove = best_move(moves)
            board = nextMove['board']
            
        if(currentPlayer):  #pass control to other player
            currentPlayer = 0
        else:
            currentPlayer = 1

        print_board(board_to_response(board),nextMove)  #print the current board for next player to move from
        print
        
    if(currentPlayer):  #pass control to other player
        currentPlayer = 0
    else:
        currentPlayer = 1
    print "Game over!  Winner was player " + str(currentPlayer + 1)
    
    

play_game(BOARD,("player","robot"))
#play_game_robotVRobot(BOARD)
