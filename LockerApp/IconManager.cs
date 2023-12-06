using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LockerApp
{
    public class IconManager
    {
        private Icon lockerIcon;
        private Icon defaultIcon;

        public IconManager(string lockerIconPath)
        {
            lockerIcon = LoadIconFromFile(lockerIconPath);
        }

        private Icon LoadIconFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                return new Icon(filePath);
            }
            else
            {
                throw new FileNotFoundException($"Icon file not found at path: {filePath}");
            }
        }

        public void SetLockerIcon(Form form)
        {
            defaultIcon = form.Icon;
            form.Icon = lockerIcon;
        }

        public void SetDefaultIcon(Form form)
        {
            form.Icon = defaultIcon;
        }
    }
}
