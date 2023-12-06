using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace LockerApp

{
    public partial class MainForm : Form

    {
        private IconManager iconManager;
        public MainForm()
        {
            InitializeComponent();
            //iconManager = new IconManager("C:\\Users\\Lenovo\\source\\repos\\LockerApp\\LockerApp\\lock.ico");
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    folderPathTextBox.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void lockButton_Click(object sender, EventArgs e)
        {
            string folderPath = folderPathTextBox.Text;
            string password = passwordTextBox.Text;

            if (string.IsNullOrEmpty(folderPath) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Фолдероо сонгож нууц үгээ хийнэ үү.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte[] key = GenerateKey(password);
            EncryptFolder(folderPath, key);
            MakeFolderNotAccessible(folderPath);
            // Hide the folder
            //File.SetAttributes(folderPath, File.GetAttributes(folderPath) | FileAttributes.Hidden);
            //iconManager.SetLockerIcon(this);
            MessageBox.Show("Фолдер амжилттай нууцлагдлаа.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private byte[] GenerateKey(string password)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("SALT1234"), 10000))
            {
                return deriveBytes.GetBytes(32);
            }
        }

        private void unlockButton_Click(object sender, EventArgs e)
        {
            string folderPath = folderPathTextBox.Text;
            string password = passwordTextBox.Text;

            if (string.IsNullOrEmpty(folderPath) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Фолдероо сонгож нууц үгээ оруулна уу", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte[] key = GenerateKey(password);

            MakeFolderAccessible(folderPath);
            DecryptFolder(folderPath, key);
            // Show the folder
            //File.SetAttributes(folderPath, FileAttributes.Normal);
            //iconManager.SetDefaultIcon(this);
            MessageBox.Show("Фолдер амжилттай нээгдлээ.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EncryptFolder(string folderPath, byte[] key)
        {
            try
            {
                string[] files = Directory.GetFiles(folderPath);

                foreach (string file in files)
                {
                    using (FileStream inputFileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        using (Aes aesAlg = Aes.Create())
                        {
                            aesAlg.Key = key;
                            aesAlg.IV = new byte[16];

                            using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                            {
                                using (CryptoStream cryptoStream = new CryptoStream(inputFileStream, encryptor, CryptoStreamMode.Read))
                                {
                                    string encryptedFilePath = Path.Combine(folderPath, Path.GetFileNameWithoutExtension(file) + ".encrypted");
                                    using (FileStream encryptedFileStream = new FileStream(encryptedFilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        cryptoStream.CopyTo(encryptedFileStream);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error encrypting folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DecryptFolder(string folderPath, byte[] key)
        {
            try
            {
                string[] files = Directory.GetFiles(folderPath);

                foreach (string file in files)
                {
                    if (file.EndsWith(".encrypted", StringComparison.OrdinalIgnoreCase))
                    {
                        using (FileStream inputFileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            using (Aes aesAlg = Aes.Create())
                            {
                                aesAlg.Key = key;
                                aesAlg.IV = new byte[16];
                                aesAlg.Padding = PaddingMode.PKCS7; // padding

                                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                                {
                                    using (CryptoStream cryptoStream = new CryptoStream(inputFileStream, decryptor, CryptoStreamMode.Read))
                                    {
                                        
                                        string decryptedFilePath = Path.Combine(folderPath, Path.GetFileNameWithoutExtension(file) + ".decrypted");
                                        using (FileStream decryptedFileStream = new FileStream(decryptedFilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            cryptoStream.CopyTo(decryptedFileStream);
                                        }
                                    }
                                }
                            }
                        }

                        //encrypted file-iig ustgah
                        File.Delete(file);
                        string decryptedFilePath1 = Path.Combine(folderPath, Path.GetFileNameWithoutExtension(file) + ".decrypted");
                        File.Delete(decryptedFilePath1);
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error decrypting folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void MakeFolderNotAccessible(string folderPath)
        {
            try
            {
                // odoogiin directory security settings-iig ni awna
                DirectorySecurity directorySecurity = new DirectorySecurity(folderPath, AccessControlSections.Access);

                // newtreh durmuudiig ustgah
                AuthorizationRuleCollection accessRules = directorySecurity.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
                foreach (FileSystemAccessRule rule in accessRules)
                {
                    directorySecurity.RemoveAccessRule(rule);
                }

                // hun bur newterch bolohgui gesen durem uusgene
                FileSystemAccessRule denyAccessRule = new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Deny);
                directorySecurity.AddAccessRule(denyAccessRule);

                // uurchilsun tohirgoogoo folder too heregjuulne
                Directory.SetAccessControl(folderPath, directorySecurity);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error making folder not accessible: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //iconManager.SetLockerIcon(this);
        }

        private void MakeFolderAccessible(string folderPath)
        {
            try
            {
                // odoogiin directory security settings-iig ni awna
                DirectorySecurity directorySecurity = new DirectorySecurity(folderPath, AccessControlSections.Access);

                // newtreh durmuudiig ustgah
                AuthorizationRuleCollection accessRules = directorySecurity.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
                foreach (FileSystemAccessRule rule in accessRules)
                {
                    if (rule.AccessControlType == AccessControlType.Deny)
                    {
                        directorySecurity.RemoveAccessRule(rule);
                    }
                }

                // newterch bolno gesen durem ugnu
                Directory.SetAccessControl(folderPath, directorySecurity);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error making folder accessible: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }


}
