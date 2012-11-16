function[] = imagetoboardmanual()

path = 'WorkingWithDepthData\bin\Release\';
path2 = 'C:\\Users\Public\Documents\GamesCrafters\';
path3 = 'C:\\Users\Kevin\Dropbox\Public';
path4 = 'C:\\Users\Kevin\Dropbox\KKShared';
outputfile = [path4 , 'output.txt'];
colorfile = [path , 'output.jpg'];
%depthfile = [path , 'depth.jpg']



colorimage = imread(colorfile);
%depthimage = imread(depthfile);
imagealpha = 0.2;

disp('Calibration (1): Click on the upper left corner slot')
disp('Calibration (2): Click on the upper right corner slot')
disp('Calibration (3): Click on the lower left corner slot')
disp('Calibration (4): Click on the lower right corner slot')

imshow(colorimage);
[i,j] = ginput(4);
    
cul = [i(1),j(1)];
cur = [i(2),j(2)];
cll = [i(3),j(3)];
clr = [i(4),j(4)];

bs = helper(colorimage,cul,cur,cll,clr);    
dlmwrite(outputfile,bs,'');

while length(strfind(bs,' '))>0
colorimage = imread(colorfile);
%depthimage = imread(depthfile);
bs = helper(colorimage,cul,cur,cll,clr)
dlmwrite(outputfile,bs,'');
pause(1);
end

function [bs] = helper(colorimage,cul,cur,cll,clr) %Convert board colorimage to board string
    
    colorimage = imadjust(colorimage, [0 1], [0 1]);
    
    disp('Converting image to string')
    boardsize = [6 7];
    bs = '';
    sz = size(colorimage);
    x = sz(2);
    y = sz(1);
    z = sz(3);
    rows = boardsize(1);
    columns = boardsize(2);
    if z ~= 3
        disp('Warning: Wrong Format. images must be in .jpg format.')
    end
    
    cl = interpolate(cul,cll,rows);   
    cr = interpolate(cur,clr,rows);
    
    points = zeros(columns,rows,2);
    pieces = cell(columns,rows);
    
    for i=1:rows 
        points(:,i,:) = interpolate(cl(i,:,:),cr(i,:,:),columns);
    end
    
    for i = 1:rows
        for j = 1:columns
            temp = round(points(j,i,:));
            pieces(j,i) = {color(colorimage(temp(2),temp(1),:))};
        end
    end
    
    imshow(colorimage);
    
    hold on
    
    for i = rows:-1:1
        for j = 1:columns
            %c = pieces(j,i);
            p = round(points(j,i,:));
            s = piece(p(1),p(2));
            bs = [bs, s];
            plot(p(1),p(2),type(s));
        end
    end
    
    hold off

function [s] = type(p) %Piece to Graphical Debugger
    if p == 'X'
        s = 'wx';
        return
    end
    if p == 'O'
        s = 'wo';
        return
    end
    if p == ' '
        s = 'w+';
        return
    end
end

function [piece] = piece(x,y)   
c = '';
%Record Red, Black, and White pixels in c.
for ii = -4:2:4
    for jj = -4:2:4
    %Check Depth Image First.
    %depthpixel = color(depthimage(y+jj,x+ii,:));
    %if depthpixel == 'W'
    %    pixel = depthpixel;
    %Check Color Image if Depth image ambiguous.
    %else if depthpixel =='D'
    colorpixel = color(colorimage(y+jj,x+ii,:));
    pixel = colorpixel;
    c = [c,pixel];
    end
end
%Count number of red, black, and white pixels.
xp = length(strfind(c,'R'));
op = length(strfind(c,'D'));
bp = length(strfind(c,'W'));
%Select the piece with the largest color count
[m,n] = max([xp,op,bp]);
pa = ['X','O',' '];
piece = pa(n);
end

function [char] = color(i) %Color Detection
    h = 96;
    l = 64;
    if i(1)>h & i(2)<l & i(3)<l
        char = 'R'; %Red
        return
    end
    if i(1)>h & i(2)>h & i(3)<l
        char = 'Y'; %Yellow
        return
    end
    if i(1)<l & i(2)>h & i(3)<l
        char = 'G'; %Green
        return
    end
    if i(1)<l & i(2)>h & i(3)>h
        char = 'C'; %Cyan
        return
    end
    if i(1)<l & i(2)<l & i(3)>h
        char = 'B'; %Blue
        return
    end
    if i(1)>h & i(2)<l & i(3)>h
        char = 'M'; %Magenta
        return 
    end
    if i(1)<l & i(2)<l & i(3)<l
        char = 'D'; %Black
        return 
    end
    if i(1)>h & i(2)>h & i(3)>h
        char = 'W'; %White
        return 
    end
    char = 'F'; %Unknown
end    
function [row] = interpolate(c1,c2,n) % Interpolator n = number of points
if n >=2
    a = 1:n;
    b = 1:n;
    for iii = 1:n
    a(iii) = c1(1)+(iii-1)*(c2(1)-c1(1))/(n-1);
    b(iii) = c1(2)+(iii-1)*(c2(2)-c1(2))/(n-1);
    end
    row = zeros(n,1,2);
    row(:,1,1) = a;
    row(:,1,2) = b;
    return
end
error('n must be at least 2')
end
end
function [] = imshow2(img1,img2)
imshow(img1);
hold on
h = imshow(img2); % Save the handle; we'll need it later
hold off
alpha(h,imagealpha);
end
end