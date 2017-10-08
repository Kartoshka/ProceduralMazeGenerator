# Procedural Maze Generation

This is a game built for COMP521 at McGill University.

It features a procedurally generated maze which is different at every playthrough.

[Playable build](https://eliotie.itch.io/maze-gen)

## Features
- Procedurally generated maze
- Full 3d environment
- Enemies traversing the maze 

## Algorithm 
The maze generation algorithm is based on [an article by Bob Nystrom](http://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/).

- It first attempts to place N rooms of size MxM in the space.
- It then places a single cell for the entry of the maze and one for the exit.
- Then, we use a recursive backtracking algorithm to fill the empty space with a spanning tree. 
- We pick random unvisited points and call the recursive algorithm until all empty spaces are full
- We then create a single door between every room and one of the hall ways near it and also with the start and end point.
- We then start caving in dead ends by start at a dead end and removing cells until we hit an intersecction.
- We cave dead ends until we are left with a specified amount

## Screenshots
//TODO
![img](https://i.imgur.com/D0bhK5f.png)
![img](https://i.imgur.com/POZ2sV7.png)
![img](https://img.itch.zone/aW1hZ2UvMTc5NDU5LzgzODI0NC5wbmc=/original/3pMGvR.png)
![img](https://img.itch.zone/aW1hZ2UvMTc5NDU5LzgzODI0NS5wbmc=/original/mAe2AI.png)
