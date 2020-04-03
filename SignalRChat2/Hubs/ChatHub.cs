using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;


namespace SignalRChat2.Hubs
{
    public class ChatHub : Hub
    {
        public cell[] oldBoard = new cell[324];
        public cell[] newBoard = new cell[324];
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendMessage2(string jsonString)
        {
            //populate oldBoard      

            for (int i = 0; i < oldBoard.Length; ++i)
            {
                oldBoard[i] = new cell();
            }

            //populate newBoard blank

            for (int i = 0; i < newBoard.Length; ++i)
            {
                newBoard[i] = new cell();
                newBoard[i].life = 0;
                newBoard[i].color = "#ffffff";
            }


            //add in cell data for the gameboard from the jsonString to oldBoard

            int count = 0;

            using (JsonDocument document = JsonDocument.Parse(jsonString))
            {
                JsonElement root = document.RootElement;
                //JsonElement studentsElement = root.GetProperty("Students");
                foreach (JsonElement cell in root.EnumerateArray())
                {
                    cell.TryGetProperty("life", out JsonElement lifeElement);
                    oldBoard[count].life = lifeElement.GetInt32();
                    //Console.WriteLine(lifeElement.ToString());
                    cell.TryGetProperty("color", out JsonElement colorElement);
                    oldBoard[count].color = colorElement.ToString();
                    //Console.WriteLine(colorElement.ToString());
                    count++;
                }
            }

            checkBoard(oldBoard);

            ///receive board in json
            ///deserialize the board from json
            ///do board calculations on a new board
            ///serialize the new board back into json
            ///send to all clients

            jsonString = JsonSerializer.Serialize(newBoard);

            await Clients.All.SendAsync("ReceiveMessage", jsonString);
        }

        //hex color to be in the format "#000000"
        public string avgHexColor(string v1, string v2)
        {
            int red1 = int.Parse(v1.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            int green1 = int.Parse(v1.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            int blue1 = int.Parse(v1.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

            int red2 = int.Parse(v2.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            int green2 = int.Parse(v2.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            int blue2 = int.Parse(v2.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

            string avgColor = "#" + ((red1+red2) / 2).ToString("X2") + ((green1+green2) / 2).ToString("X2") + ((blue1+blue2) / 2).ToString("X2");
            return avgColor;
        }

        public int[] addHexColor(int[] v1, int[] v2)
        {
            //int red1 = int.Parse(v1.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            //int green1 = int.Parse(v1.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            //int blue1 = int.Parse(v1.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

            //int red2 = int.Parse(v2.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            //int green2 = int.Parse(v2.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            //int blue2 = int.Parse(v2.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

            //string addColor = "#" + (red1 + red2).ToString("X2") + (green1 + green2).ToString("X2") + (blue1 + blue2).ToString("X2");
            int[] addColor = { (v1[0] + v2[0]), (v1[1] + v2[1]), (v1[2] + v2[2]) };
            return addColor;
        }


        public string divHexColor(int[] a1, int count)
        {
            if (count > 0)
            {

                //int red1 = int.Parse(v1.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                //int green1 = int.Parse(v1.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                //int blue1 = int.Parse(v1.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

                string divColor = "#" + (a1[0] / count).ToString("X2") + (a1[1] / count).ToString("X2") + (a1[2] / count).ToString("X2");
                return divColor;
            } else
            {
                return "#" + a1[0].ToString("X2") + a1[1].ToString("X2") + a1[2].ToString("X2");
            }
        }


        public int[] colorHexToIntArray(string v1)
        {

            int red1 = int.Parse(v1.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            int green1 = int.Parse(v1.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            int blue1 = int.Parse(v1.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

            int[] intColor = { red1, green1, blue1 };

            return intColor;

        }


        ///receive board, and index of cell return count of horizontal live cells
        public int[] horCheck(cell[] board, int i)
        {
            int count = 0;

            //if (board[i].life == 1) { string hexColor = board[i].color; }
            //string[] hexArr = new string[2];
            //string hexColor = "#000000";
            int[] hexColor = { 00, 00, 00 };
            
            if (board[i + 1].life == 1)
            {
                count++;

                //hexArr[0] = board[i + 1].color;
                hexColor = addHexColor(colorHexToIntArray(board[i + 1].color), hexColor);
            }
            if (board[i - 1].life == 1)
            {
                hexColor = addHexColor(colorHexToIntArray(board[i - 1].color), hexColor);
                //hexArr[1] = board[i + 1].color;
                //hexColor = avgHexColor(board[i - 1].color, hexColor);
                count++;
            }
            int[] retArr = new int[hexColor.Length + 1];
            hexColor.CopyTo(retArr, 1);
            retArr[0] = count;
            return retArr;
        }





        ///return a count of how many vertical cells are live
        public int[] vertCheck(cell[] board, int i)
        {
            int count = 0;
            //string hexColor = board[i].color;
            //string hexColor = "#000000";
            int[] hexColor = { 00, 00, 00 };
            if (board[i + 18].life == 1)
            {
                hexColor = addHexColor(colorHexToIntArray(board[i + 18].color), hexColor);
                //hexColor = addHexColor(board[i + 18].color, hexColor);
                //hexColor = avgHexColor(board[i + 18].color, hexColor);
                count++;
            }
            if (board[i - 18].life == 1)
            {
                hexColor = addHexColor(colorHexToIntArray(board[i - 18].color), hexColor);
                //hexColor = addHexColor(board[i - 18].color, hexColor);
                //hexColor = avgHexColor(board[i - 18].color, hexColor);
                count++;
            }
            //string[] retArr = { count.ToString(), hexColor };


            int[] retArr = new int[hexColor.Length + 1];
            hexColor.CopyTo(retArr, 1);
            retArr[0] = count;
            return retArr;

            //return retArr;
        }

        ///check if two cells are diagnoally adjacent

        public int[] diagCheck(cell[] board, int i)
        {
            int count = 0;
            //string hexColor = "#000000";
            int[] hexColor = { 00, 00, 00 };
            //string hexColor = board[i].color;
            if (board[i + 19].life == 1)
            {
                hexColor = addHexColor(colorHexToIntArray(board[i + 19].color), hexColor);
                //hexColor = addHexColor(board[i + 19].color, hexColor);
                //hexColor = avgHexColor(board[i + 19].color, hexColor);
                count++;
            }
            if (board[i + 17].life == 1)
            {
                hexColor = addHexColor(colorHexToIntArray(board[i + 17].color), hexColor);
                //hexColor = addHexColor(board[i + 17].color, hexColor);
                //hexColor = avgHexColor(board[i + 17].color, hexColor);
                count++;
            }
            if (board[i - 19].life == 1)
            {
                hexColor = addHexColor(colorHexToIntArray(board[i - 19].color), hexColor);
                //hexColor = addHexColor(board[i - 19].color, hexColor);
                //hexColor = avgHexColor(board[i - 19].color, hexColor);
                count++;
            }
            if (board[i - 17].life == 1)
            {
                hexColor = addHexColor(colorHexToIntArray(board[i - 17].color), hexColor);
                //hexColor = addHexColor(board[i - 17].color, hexColor);
                //hexColor = avgHexColor(board[i - 17].color, hexColor);
                count++;
            }
            int[] retArr = new int[hexColor.Length + 1];
            hexColor.CopyTo(retArr, 1);
            retArr[0] = count;
            
            return retArr;
        }

        ///game logic:
        //loop through all relevent cells:

        public void checkBoard(cell[] board)
        {

            ///board is 16 by 16, with one extra row and column around the board
            ///makeing the board 18 by 18

            //start at this cell, the first relevent cell
            int x = 19;

            //go until the second to last row
            while (x < 18 * 16)
            {
                if (x % 18 == 0 || x % 18 == 17)
                {
                    x++;
                }
                else
                {
                    //valid cells, check:
                    //1 Any live cell with two or three neighbors survives.
                    //2 Any dead cell with three live neighbors becomes a live cell.
                    //3 All other live cells die in the next generation.Similarly, all other dead cells stay dead
                    int[] tempColor = new int[3];
                    string strHexColor;
                    int[] currArr = horCheck(board, x);
                    int liveNeighbors = currArr[0];
                    int[] hexColor = { currArr[1], currArr[2], currArr[3] };

                    currArr = vertCheck(board, x);
                    liveNeighbors += currArr[0];
                    tempColor[0] = currArr[1];
                    tempColor[1] = currArr[2];
                    tempColor[2] = currArr[3];

                    hexColor = addHexColor(tempColor, hexColor);

                    currArr = diagCheck(board, x);
                    liveNeighbors += (currArr[0]);
                    //hexColor = addHexColor(currArr[1], hexColor);
                    tempColor[0] = currArr[1];
                    tempColor[1] = currArr[2];
                    tempColor[2] = currArr[3];
                    hexColor = addHexColor(tempColor, hexColor);
                    //hexColor = avgHexColor(currArr[1], hexColor);

                    if (board[x].life == 1)
                    {
                        hexColor = addHexColor(hexColor, colorHexToIntArray(board[x].color));

                        strHexColor = divHexColor(hexColor, liveNeighbors+1);
                    }
                    else
                    {
                        strHexColor = divHexColor(hexColor, liveNeighbors);
                    }
                    

                    //int liveNeighbors = horCheck(board, x) + vertCheck(board, x) + diagCheck(board, x);



                    if (board[x].life == 1)
                    {
                        //if live cell

                        if (liveNeighbors == 2 || liveNeighbors == 3)
                        {
                            //live cell stays alive
                            newBoard[x].life = 1;
                            newBoard[x].color = strHexColor;
                        }
                        else
                        {
                            //live cell dies
                            newBoard[x].life = 0;
                            newBoard[x].color = "#ffffff";
                        }
                    }
                    else
                    {
                        //currently dead cell
                        if (liveNeighbors == 3)
                        {
                            //becomes alive
                            newBoard[x].life = 1;
                            newBoard[x].color = strHexColor;
                        }
                        else
                        {
                            newBoard[x].life = 0;
                            newBoard[x].color = "#ffffff";
                        }
                    }
                    x++;
                }
            }
            ///example/////////////////
            //int x = 19;
            //while (x < 18 * 17)
            //{
            //    if (x % 18 == 0 || x % 18 == 17)
            //    {
            //        x++;
            //    }
            //    else
            //    {
            //        ///check cell

            //    }
            //}
            ///example/////////////////
        }
    }
}
