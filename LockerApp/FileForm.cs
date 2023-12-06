using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace LockerApp
{
    public partial class FileForm : Form
    {
        public FileForm()
        {
            InitializeComponent();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePathTextBox.Text = openFileDialog.FileName;
                }
            }
        }

        private void lockButton_Click(object sender, EventArgs e)
        {
            string filePath = filePathTextBox.Text;
            string password = passwordTextBox.Text;

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please select a file and enter a password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte[] key = GenerateKey(password);

            try
            {
                EncryptFile(filePath, key);
                MessageBox.Show($"File locked successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error locking file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void unlockButton_Click(object sender, EventArgs e)
        {
            string filePath = filePathTextBox.Text;
            string password = passwordTextBox.Text;

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please select a file and enter a password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte[] key = GenerateKey(password);

            try
            {
                DecryptFile(filePath, key);
                MessageBox.Show($"File unlocked successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error unlocking file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private byte[] GenerateKey(string password)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("SALT1234"), 10000))
            {
                return deriveBytes.GetBytes(32);
            }
        }

        private void EncryptFile(string filePath, byte[] key)
        {
            try
            {
                using (FileStream inputFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = new byte[16];

                    string encryptedFilePath = filePath + ".encrypted";
                    using (FileStream encryptedFileStream = new FileStream(encryptedFilePath, FileMode.Create, FileAccess.Write))
                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    using (CryptoStream cryptoStream = new CryptoStream(encryptedFileStream, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead;
                        while ((bytesRead = inputFileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            cryptoStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }

                File.Delete(filePath); // Delete the original file after encryption
            }
            catch (Exception ex)
            {
                throw new Exception($"Error encrypting file: {ex.Message}");
            }
        }

        private void DecryptFile(string filePath, byte[] key)
        {
            try
            {
                if (filePath.EndsWith(".encrypted", StringComparison.OrdinalIgnoreCase))
                {
                    using (FileStream inputFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (Aes aesAlg = Aes.Create())
                        {
                            aesAlg.Key = key;
                            aesAlg.IV = new byte[16];
                            aesAlg.Padding = PaddingMode.PKCS7; // Explicitly set the padding mode

                            using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                            {
                                using (CryptoStream cryptoStream = new CryptoStream(inputFileStream, decryptor, CryptoStreamMode.Read))
                                {
                                    // Create a new file with the original extension to store the decrypted data
                                    string originalExtension = Path.GetExtension(filePath);
                                    string decryptedFilePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_decrypted" + originalExtension);
                                    using (FileStream decryptedFileStream = new FileStream(decryptedFilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        cryptoStream.CopyTo(decryptedFileStream);
                                    }
                                }
                            }
                        }
                    }

                    // Delete the original encrypted file
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error decrypting file: {ex.Message}");
            }
        }





    }
}