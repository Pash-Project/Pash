using System;

namespace System.Management
{
    public class PathNavigation
    {
        public static Path CalculateFullPath(Path currentLocation, Path changeCommand)
        {
            changeCommand = (changeCommand ?? string.Empty).NormalizeSlashes();
            currentLocation = currentLocation.NormalizeSlashes();

            bool applyParts = false;
            Path resultPath;

            // use the input 'changeCommand' path if it's 
            // 'rooted' otherwise we go from the currentLocation
            if(changeCommand.HasDrive())
            {
                resultPath = changeCommand;
            }
            else 
            {
                applyParts = true;
                resultPath = currentLocation;
            }

            var correctSeparator = Char.Parse(currentLocation.CorrectSlash);
            var changeParts = changeCommand.ToString().Split(correctSeparator);

            foreach(var part in changeParts)
            {
                // ignore single dot as it does nothing...
                if(part == ".")
                {
                    continue;
                }

                if(part == "..")
                {
                    resultPath = resultPath.GetParentPath(currentLocation.GetDrive());
                }
                else if(applyParts)
                {
                    resultPath = resultPath.Combine(part);
                }
            }

            return resultPath.ApplyDriveSlash();
        }
    }
}
