using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.XR;
using static Unity.Collections.AllocatorManager;

public enum PlayerOption
{
    NONE, //0
    X, // 1
    O // 2
}

public class TTT : MonoBehaviour
{
    public int Rows;
    public int Columns;
    [SerializeField] BoardView board;

    PlayerOption currentPlayer = PlayerOption.X;
    Cell[,] cells;

    // Start is called before the first frame update
    void Start()
    {
        cells = new Cell[Columns, Rows];

        board.InitializeBoard(Columns, Rows);

        for(int i = 0; i < Rows; i++)
        {
            for(int j = 0; j < Columns; j++)
            {
                cells[j, i] = new Cell();
                cells[j, i].current = PlayerOption.NONE;
            }
        }
    }

    public void MakeOptimalMove()
    {
        // find whos oponenet 
        PlayerOption opponent;
        //current player is x
        if (currentPlayer == PlayerOption.X)
        {
            //opponent is o
            opponent = PlayerOption.O;
        }
        else
        {
            //current player is o opponent is x
            opponent = PlayerOption.X;
        }
        
        //INSTRUCTIONS If the computer can win(has two in a row, and the third space is open), it should do so.
        for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    //test empty spaces
                    if (cells[column, row].current == PlayerOption.NONE)
                    {
                        //attempt a move 
                        cells[column, row].current = currentPlayer;
                        //if move would win
                        if (GetWinner() == currentPlayer)
                        {
                            //remove attempt
                            cells[column, row].current = PlayerOption.NONE;
                            //make permanent move
                            ChooseSpace(column, row);
                            return;
                        }
                        //move does not win remove attempt
                        cells[column, row].current = PlayerOption.NONE;
                    }

                }
            }

        //INSTRUCTIONS If the opponent has two in a row, and the third space is open, block them to prevent victory.
        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                //empty space s
                if (cells[column, row].current == PlayerOption.NONE)
                {
                    //test oponent move on empty space
                    cells[column, row].current = opponent;

                    //check if itd win
                    if (GetWinner() == opponent)
                    {
                        //remove test
                        cells[column, row].current = PlayerOption.NONE;
                        //mark permanent move (not oponents) 
                        ChooseSpace(column, row);
                        return;
                    }
                    //opponent move woulnt win remove test
                    cells[column, row].current = PlayerOption.NONE;
                }
            }
        }

        //INSTRUCTIONS If the board is empty, it is advantageous to take a corner
        bool emptyBoard = true;

        //check board if empty
        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                //space occupied, board not empty 
                if (cells[column,row].current != PlayerOption.NONE)
                {
                    emptyBoard = false;
                }
            }

        }
        //emptY board, take random corner
        if (emptyBoard)
        {
            //random number /4 for corners
            int randomCorner = Random.Range(0, 4);

            //choose corner based on random num
            switch (randomCorner)
            {
                //top left 
                case 0:
                    ChooseSpace(0, 0);
                    break;

                //top right
                case 1:
                    ChooseSpace(2, 0);
                    break;

                //bottom left
                case 2:
                    ChooseSpace(0, 2);
                    break;

                //bottom right
                case 3:
                    ChooseSpace(2, 2);
                    break;
            }
            return;
        }

        //INSTRUCTIONS If the opponent controls a corner, but the center is open, take the center
        bool emptyCenter = cells[1, 1].current == PlayerOption.NONE;
        bool opponentCorner = cells[0, 0].current == opponent || cells[2, 0].current == opponent || cells[0, 2].current == opponent || cells[2, 2].current == opponent;

        //both opponent has corner and middle is free
        if (opponentCorner && emptyCenter)
        {
            //choose middle
            ChooseSpace(1, 1);
            return;
        }

        //INSTRUCTIONS If a player controls a corner, but not the center, they should take a cell adjacent to the corner they control
        bool playerCorner = cells[0, 0].current == currentPlayer || cells[2, 0].current == currentPlayer || cells[0, 2].current == currentPlayer || cells[2, 2].current == currentPlayer;
        bool playerCenter = cells[1, 1].current == currentPlayer;

        //current player has corner but not center
        if (playerCorner && !playerCenter)
        {
            //check top left corner
            if (cells[0, 0].current == currentPlayer) 
            {
                //try to the side, if empty chose space
                if (cells[1,0].current==PlayerOption.NONE)
                {
                    ChooseSpace(1, 0);
                    return;
                }
                //try down, if empty chose space
                if (cells[0, 1].current == PlayerOption.NONE)
                {
                    ChooseSpace(0, 1);
                    return;
                }
            }

            //check top righ tcorner
            if (cells[2, 0].current == currentPlayer)
            {
                //try to the side, if empty chose space
                if (cells[1, 0].current == PlayerOption.NONE)
                {
                    ChooseSpace(1, 0);
                    return;
                }
                //try down, if empty chose space
                if (cells[2, 1].current == PlayerOption.NONE)
                {
                    ChooseSpace(2, 1);
                    return;
                }
            }

            //check bottom left corner 
            if (cells[0, 2].current == currentPlayer)
            {
                //try to the side, if empty chose space
                if (cells[1, 2].current == PlayerOption.NONE)
                {
                    ChooseSpace(1, 2);
                    return;
                }
                //try up, if empty chose space
                if (cells[0, 1].current == PlayerOption.NONE)
                {
                    ChooseSpace(0, 1);
                    return;
                }
            }

            //check bottom right corner
            if (cells[2, 2].current == currentPlayer)
            {
                //try to the side, if empty chose space
                if (cells[1, 2].current == PlayerOption.NONE)
                {
                    ChooseSpace(1, 2);
                    return;
                }
                //try up, if empty chose space
                if (cells[2, 1].current == PlayerOption.NONE)
                {
                    ChooseSpace(2, 1);
                    return;
                }
            }
        }

        //INSTRUCTIONS as a fail - safe, if none of the above happens, take a random cell
        //check for any empty space
        bool emptySpace = false;
        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                //at least one empty
                if (cells[column, row].current == PlayerOption.NONE)
                {
                    emptySpace = true;
                    break;
                }
            }
            //no empty spaces left
            if (emptySpace)
            {
                break;
            }
        }
        //look for random empty space to choose if it exists
        if(emptySpace)
        {
            while (true)
            {
                int column = Random.Range(0, Columns);
                int row = Random.Range(0, Rows);

                //chose empty space place mark
                if (cells[column, row].current == PlayerOption.NONE)
                {
                    ChooseSpace(column, row);
                    break;
                }
            }
        }
    }

    public void ChooseSpace(int column, int row)
    {
        // can't choose space if game is over
        if (GetWinner() != PlayerOption.NONE)
            return;

        // can't choose a space that's already taken
        if (cells[column, row].current != PlayerOption.NONE)
            return;

        // set the cell to the player's mark
        cells[column, row].current = currentPlayer;

        // update the visual to display X or O
        board.UpdateCellVisual(column, row, currentPlayer);

        // if there's no winner, keep playing, otherwise end the game
        if(GetWinner() == PlayerOption.NONE)
            EndTurn();
        else
        {
            Debug.Log("GAME OVER!");
        }
    }

    public void EndTurn()
    {
        // increment player, if it goes over player 2, loop back to player 1
        currentPlayer += 1;
        if ((int)currentPlayer > 2)
            currentPlayer = PlayerOption.X;
    }

    public PlayerOption GetWinner()
    {
        // sum each row/column based on what's in each cell X = 1, O = -1, blank = 0
        // we have a winner if the sum = 3 (X) or -3 (O)
        int sum = 0;

        // check rows
        for (int i = 0; i < Rows; i++)
        {
            sum = 0;
            for (int j = 0; j < Columns; j++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;

        }

        // check columns
        for (int j = 0; j < Columns; j++)
        {
            sum = 0;
            for (int i = 0; i < Rows; i++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;

        }

        // check diagonals
        // top left to bottom right
        sum = 0;
        for(int i = 0; i < Rows; i++)
        {
            int value = 0;
            if (cells[i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        // top right to bottom left
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            int value = 0;

            if (cells[Columns - 1 - i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[Columns - 1 - i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        return PlayerOption.NONE;
    }
}
