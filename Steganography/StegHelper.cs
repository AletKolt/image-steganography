using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Steganography_Using_LSB
{
    public class StegHelper
    {
        //this is an enumerator data type that we will use to track the state of our operation, whether we are "Hiding" or "Filling with zeros"
        //Hiding is when the text characters converted to integers are being saved into the LSB of the image
        //Filling with zeros is when all the text has been saved into the LSB and then we are inputting 8 zeros to keep track of where we stopped.
        public enum State
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap EmbedText(string text, Bitmap bmp)
        {
            // initially, we'll be hiding characters in the image
            State state = State.Hiding;

            // holds the index (the place of the character in the text that is to be saved into the image) of the character that is being hidden
            int charIndex = 0;

            // holds the value of the character converted to integer
            int charValue = 0;

            // holds the index of the color element (R or G or B) that is currently being processed
            long pixelElementIndex = 0;

            // holds the number of trailing zeros that have been added when finishing the process
            int zeros = 0;

            // hold pixel elements
            int R = 0, G = 0, B = 0;

            // pass through the rows
            //here, we loop through the rows of the image 
            for (int i = 0; i < bmp.Height; i++)
            {
                // at the same time, we loop through the columns of the image
                for (int j = 0; j < bmp.Width; j++)
                {
                    // holds the pixel that is currently being processed
                    Color pixel = bmp.GetPixel(j, i);

                    // now, clear the least significant bit (LSB) from each pixel element
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    // for each pixel, pass through its elements (RGB)
                    for (int n = 0; n < 3; n++)
                    {
                        // check if new 8 bits has been processed. We check whether the 8-bits of the character value that has been converted to integer has been processed. When we find the modulus of the current number by 8 and it gives us 0 then it means the number is a multiple of 8 which signifies that 8-bit processing of a character has completed.
                        if (pixelElementIndex % 8 == 0)
                        {
                            // check if the whole process has finished
                            // we can say that it's finished when 8 zeros are added
                            //We are checking the state first by checking whether the state is now "Filling with zeros" AND that the number of the zeros integer declared at the beginning of this program has amounted to 8. IF it has, then we have reached the end of inserting the character into the image.
                            //If the conditions above are not met, the code continues from the next statements.
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                // apply the last pixel on the image
                                // even if only a part of its elements have been affected
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                // return the bitmap with the text hidden in
                                return bmp;
                            }


                            //Since, the State is still Hiding and zeros is not equal to 8, we continue our program.
                            // check if all characters has been hidden
                            //We do this check by comparing whether character index i.e the position of the character in the text that is to be hidden is greater than the length of the text itself. If it is greater, then it means all the characters in the text has been encoded into the image.
                            if (charIndex >= text.Length)
                            {
                                // start adding zeros to mark the end of the text
                                //Once all characters has been encoded, then start filling with zeros, to indicate where the message ends.
                                state = State.Filling_With_Zeros;
                            }
                                // if the character index is less than the text length i.e there are still characters left to be encoded, then continue
                            else
                            {
                                // move to the next character and process again
                                charValue = text[charIndex++];
                            }
                        }

                        // check which pixel element has the turn to hide a bit in its LSB
                        //This switch statement tracks the modulus of three of pixel element, if the modulus gives 0 the it means the RED color of the pixel has the turn to hide a character bit
                        switch (pixelElementIndex % 3)
                        {
                            //If the ineteger returned after the 'pixelElementIndex % 3' calculation is 0 then it means that it is the turn of the Red pixel element to hide a character bit. Because if it is 0 - Red, 1 - Green, 2 - Blue.
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        // the rightmost bit in the character will be (charValue % 2)
                                        // to put this value instead of the LSB of the pixel element
                                        // just add it to it
                                        // recall that the LSB of the pixel element had been cleared
                                        // before this operation
                                        R += charValue % 2;

                                        // removes the added rightmost bit of the character
                                        // such that next time we can reach the next one
                                        charValue /= 2;
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    //The new pixel color is set here
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        //This code makes sure that our program after this loop moves to the next pixel element to continue inserting text characters
                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        {
                            // increment the value of zeros until it is 8
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string ExtractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            // holds the text that will be extracted from the image
            string extractedText = String.Empty;

            // pass through the rows
            for (int i = 0; i < bmp.Height; i++)
            {
                // pass through each row
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);

                    // for each pixel, pass through its elements (RGB)
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    // get the LSB from the pixel element (will be pixel.R % 2)
                                    // then add one bit to the right of the current character
                                    // this can be done by (charValue = charValue * 2)
                                    // replace the added bit (which value is by default 0) with
                                    // the LSB of the pixel element, simply by addition
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++;

                        // if 8 bits has been added, then add the current character to the result text
                        if (colorUnitIndex % 8 == 0)
                        {
                            // reverse? of course, since each time the process happens on the right (for simplicity)
                            charValue = reverseBits(charValue);

                            // can only be 0 if it is the stop character (the 8 zeros)
                            if (charValue == 0)
                            {
                                return extractedText;
                            }

                            // convert the character value from int to char
                            char c = (char)charValue;

                            // add the current character to the result text
                            extractedText += c.ToString();
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n)
        {
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result;
        }
    }
}
