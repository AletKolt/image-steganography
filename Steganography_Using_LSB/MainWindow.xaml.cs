using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using MessageBox = System.Windows.MessageBox;
using Steganography;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.NetworkInformation;
using System.Net.Mail;
using System.Net.Mime;

namespace Steganography_Using_LSB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void encodeImage_Loaded(object sender, RoutedEventArgs e)
        {
            imagePictureBox.BackgroundImage = System.Drawing.Image.FromFile(@"C:\Users\Kindness\documents\visual studio 2010\Projects\Steganography_Using_LSB\Steganography_Using_LSB\person_icon.png");
        }

        private void decodeTab_Loaded(object sender, RoutedEventArgs e)
        {
            //decodeImagePictureBox.BackgroundImage = System.Drawing.Image.FromFile(@"C:\Users\Kindness\documents\visual studio 2010\Projects\Steganography_Using_LSB\Steganography_Using_LSB\person_icon.png");
            //passwordStackPanel.Visibility = System.Windows.Visibility.Hidden;
        }

        private void selectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Multiselect = false;
            openDialog.RestoreDirectory = true;
            openDialog.Title = "Select a file";
            openDialog.InitialDirectory = @"C://";
            openDialog.Filter = "PNG File (*.png)|*.png|Bitmap Image (*.bmp)|*.bmp";
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!String.IsNullOrEmpty(openDialog.FileName))
                {
                    this.imagePictureBox.BackgroundImage = System.Drawing.Image.FromFile(openDialog.FileName);
                }
            }
        }

        private void loadTextFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select file";
            dialog.InitialDirectory = @"C:/";
            dialog.Filter = "Text files (*.txt)|*.txt";
            dialog.RestoreDirectory = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                messageTextBox.Text = File.ReadAllText(dialog.FileName);
            }
            else
            {
                //Do nothing if user cancels
            }
        }

        private void encodeButton_Click(object sender, RoutedEventArgs e)
        {
            EncodeMessage();

        }

        private void decodeSelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Multiselect = false;
            openDialog.RestoreDirectory = true;
            openDialog.Title = "Select a file";
            openDialog.InitialDirectory = @"C://";
            openDialog.Filter = "PNG File (*.png)|*.png|Bitmap Image (*.bmp)|*.bmp";
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!String.IsNullOrEmpty(openDialog.FileName))
                {
                    this.decodeImagePictureBox.BackgroundImage = System.Drawing.Image.FromFile(openDialog.FileName);
                }
            }
        }

        private void decodeButton_Click(object sender, RoutedEventArgs e)
        {
            Bitmap bitmap = (Bitmap)decodeImagePictureBox.BackgroundImage;
            if (decodeImagePictureBox.BackgroundImage != null)
            {
                if ((bool)decryptTextCheckBox.IsChecked)
                {
                    if (!String.IsNullOrEmpty(decodePasswordTextBox.Password))
                    {
                        string extractedEncryptedText = StegHelper.ExtractText(bitmap);
                        try
                        {
                            string extractText = Crypto.DecryptStringAES(extractedEncryptedText, decodePasswordTextBox.Password);
                            decodeMessageTextBox.Text = extractText;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(String.Format("Wrong password", ex.Message));
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter a password");

                    }
                }
                else
                {
                    decodeMessageTextBox.Text = StegHelper.ExtractText(bitmap);
                }
            }
            else
            {
                MessageBox.Show("Please select an image");
            }

        }

        private void EncodeMessage()
        {
            Bitmap bitmap = (Bitmap)imagePictureBox.BackgroundImage;
            if (!String.IsNullOrEmpty(messageTextBox.Text))
            {
                if ((bool)encryptTextCheckBox.IsChecked)
                {
                    if (!String.IsNullOrEmpty(passwordTextBox.Password))
                    {
                        try
                        {
                            string encryptedText = Crypto.EncryptStringAES(messageTextBox.Text, passwordTextBox.Password);
                            bitmap = StegHelper.EmbedText(encryptedText, bitmap);
                            SaveFile(bitmap);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("An error occurred\n" + ex.Message);
                        }

                    }
                    else
                    {
                        MessageBox.Show("Please enter a password");
                    }
                }
                else
                {
                    string message = "Do you not want to encrypt the text to be saved in the image by using a password?";
                    MessageBoxResult result = System.Windows.MessageBox.Show(message, "Information", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.None, System.Windows.MessageBoxOptions.DefaultDesktopOnly);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            encryptTextCheckBox.IsChecked = true;
                            System.Windows.MessageBox.Show("Please enter a password", "Enter password", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case MessageBoxResult.No:
                            encryptTextCheckBox.IsChecked = false;
                            StegHelper.EmbedText(messageTextBox.Text, bitmap);
                            SaveFile(bitmap);
                            break;
                    };
                }
            }
            else
            {
                MessageBox.Show("Please enter message or load message");
            }
        }

        private static void SaveFile(Bitmap bitmap)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Png Image|*.png|Bitmap Image|*.bmp";

            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                switch (saveDialog.FilterIndex)
                {
                    case 0:
                        {
                            bitmap.Save(saveDialog.FileName, ImageFormat.Png);
                        } break;
                    case 1:
                        {
                            bitmap.Save(saveDialog.FileName, ImageFormat.Bmp);
                        } break;
                }

            }
            MessageBox.Show("Image saved successfully");
        }

        //this is the method that is used to send the image as an email attchment to the crime division
        private void SendImageWithAttachment(string subject, string message, Bitmap bitmap)
        {

            if (Utility.InternetConnectionExists())
            {
                try
                {
                    MemoryStream stream = new MemoryStream();
                    bitmap.Save(stream, ImageFormat.Png);
                    stream.Position = 0;

                    SmtpClient smtp = new SmtpClient();
                    smtp.EnableSsl = true;
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    //add your password to where you see ........
                    smtp.Credentials = new System.Net.NetworkCredential("aletkolt@gmail.com", "!119900KoltAlet");

                    //ContentType contentType = new ContentType();
                    //contentType.MediaType = MediaTypeNames.Image.Gif;
                    //contentType.Name = "media";

                    MailMessage msg = new MailMessage();
                    msg.IsBodyHtml = true;
                    msg.Subject = subject;
                    Attachment attach = new Attachment(stream, string.Format("image.{0}", System.Drawing.Imaging.ImageFormat.Png));
                    msg.Attachments.Add(attach);
                    //from email address
                    msg.From = new MailAddress("aletkolt@gmail.com", "UDUS CRIME DIVISION");
                    //to email address
                    msg.To.Add(new MailAddress("aletkolt@gmail.com", "UDUS CRIME DIVISION"));
                    msg.Body = emailMessageBodyTextBox.Text;

                    smtp.Send(msg);
                    MessageBox.Show("E-mail message sent successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during sending of the mail, please try again" + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Internet connection does not exist or it is not connecting quite well with the internet to send the e-mail, please try again");
            }
        }

        private void emailSelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Multiselect = false;
            openDialog.RestoreDirectory = true;
            openDialog.Title = "Select a file";
            openDialog.InitialDirectory = @"C://";
            openDialog.Filter = "PNG File (*.png)|*.png|Bitmap Image (*.bmp)|*.bmp";
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!String.IsNullOrEmpty(openDialog.FileName))
                {
                    this.emailImagePictureBox.BackgroundImage = System.Drawing.Image.FromFile(openDialog.FileName);
                }
            }
        }

        private void emailLoadTextFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select file";
            dialog.InitialDirectory = @"C:/";
            dialog.Filter = "Text files (*.txt)|*.txt";
            dialog.RestoreDirectory = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                emailMessageTextBox.Text = File.ReadAllText(dialog.FileName);
            }
            else
            {
                //Do nothing if user cancels
            }
        }

        private void sendEncodedImageButton_Click(object sender, RoutedEventArgs e)
        {
            Bitmap bitmap = (Bitmap)emailImagePictureBox.BackgroundImage;
            if (!String.IsNullOrEmpty(emailMessageTextBox.Text))
            {
                if ((bool)emailEncryptTextCheckBox.IsChecked)
                {
                    if (!String.IsNullOrEmpty(emailPasswordTextBox.Password))
                    {
                        if (!String.IsNullOrEmpty(emailMessageSubjectTextBox.Text) && !String.IsNullOrEmpty(emailMessageBodyTextBox.Text))
                        {
                            //try
                            //{
                                string encryptedText = Crypto.EncryptStringAES(emailMessageTextBox.Text, emailPasswordTextBox.Password);
                                bitmap = StegHelper.EmbedText(encryptedText, bitmap);
                                SendImageWithAttachment(emailMessageSubjectTextBox.Text, emailMessageSubjectTextBox.Text, bitmap);
                            /*}
                            catch (Exception ex)
                            {
                                MessageBox.Show("An error occurred\n");
                            }*/
                        }
                        else
                        {
                            MessageBox.Show("Please enter message into subject and body of email");
                        }


                    }
                    else
                    {
                        MessageBox.Show("Please enter a password");
                    }
                }
                else
                {
                    //Just send without encryption
                }
            }
            else
            {
                MessageBox.Show("Please enter a message to encode into image");
            }

        }
    }
}


