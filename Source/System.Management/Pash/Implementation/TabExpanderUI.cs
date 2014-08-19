using System;
using System.Linq;
using System.Text;

namespace Mono.Terminal
{
    public delegate string[] TabExpansionEvent(string hardPrefix, string replacable);

    public class TabExpanderUI
    {
        private const int NO_ITEM_SELECTED = -1;
        private const int ASKING_USER = -2;
        private const int NOT_INITIALIZED = -3;

        private string _hardPrefix;
        private string _softPrefix;
        private string[] _expandandedItems;
        private int _selectedItem = NOT_INITIALIZED;

        private int _maxWrittenX;
        private int _maxWrittenY;

        public int RenderStartX { get; private set; }
        public int RenderStartY { get; private set; }
        public bool Running { get; private set; }
        public string AcceptedCommand { get; private set; }

        public TabExpansionEvent TabExpansionEvent;

        public TabExpanderUI()
        {
            Running = false;
        }

        public bool HandleKey(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
            case ConsoleKey.Enter:
                if (_selectedItem == ASKING_USER)
                {
                    goto case ConsoleKey.Tab;
                }
                else
                {
                    Accept();
                    return _selectedItem >= 0;
                }
            case ConsoleKey.Tab:
                ChooseNext();
                return true;
            case ConsoleKey.Escape:
                Abort(true);
                return true;
            default:
                return false;
            }
        }

        public void Start(string prefix)
        {
            Running = true;
            if (prefix.Equals(GetExpandedCommand()) && !prefix.Equals(AcceptedCommand))
            {
                return;
            }
            AcceptedCommand = null;
            int spacePos = prefix.LastIndexOf(' ');
            _hardPrefix = spacePos < 0 ? "" : prefix.Substring(0, spacePos);
            _softPrefix = prefix.Substring(spacePos + 1);
            _selectedItem = NOT_INITIALIZED;
        }

        public void Accept()
        {
            // finished
            AcceptedCommand = GetExpandedCommand();
            Running = false;
        }

        public void ChooseNext()
        {
            bool selectNextItem = true;
            if (_selectedItem == NOT_INITIALIZED)
            {
                if (TabExpansionEvent == null)
                {
                    _expandandedItems = new string[] { };
                    return;
                }
                _expandandedItems = TabExpansionEvent(_hardPrefix, _softPrefix);
                _selectedItem = NO_ITEM_SELECTED;
                selectNextItem = false;
            }
            if (_expandandedItems == null || _expandandedItems.Length < 1)
            {
                return;
            }
            else if (_expandandedItems.Length == 1)
            {
                _selectedItem = 0;
                Accept();
                return;
            }
            else if (selectNextItem)
            {
                _selectedItem++;
                _selectedItem = _selectedItem % _expandandedItems.Length;
            }
        }

        public void Abort(bool resetSelection)
        {
            Running = false;
            if (resetSelection)
            {
                _selectedItem = NOT_INITIALIZED;
            }
        }

        public string GetExpandedCommand()
        {
            string chosenPrefix;
            if (_selectedItem == NOT_INITIALIZED)
            {
                chosenPrefix = _softPrefix;
            }
            else if (_selectedItem == NO_ITEM_SELECTED)
            {
                chosenPrefix = GetCommonPrefix();
            }
            else
            {
                chosenPrefix = _expandandedItems[_selectedItem];
            }
            return String.IsNullOrEmpty(_hardPrefix) ? chosenPrefix : _hardPrefix + " " + chosenPrefix;
        }

        public void Render(int startX, int startY)
        {
            RenderStartX = startX;
            RenderStartY = startY;
            int bufferWidth = Console.BufferWidth;
            int bufferHeight = Console.BufferHeight;
            int numItems = _expandandedItems == null ? 0 : _expandandedItems.Length;

            Clear();

            if (numItems == 0 || !Running)
            {
                return;
            }

            int colwidth = _expandandedItems.Max(item => item.Length) + 1;
            if (colwidth >= bufferWidth)
            {
                colwidth = bufferWidth - 1;
            }
            int cols = bufferWidth / colwidth;
            cols = cols > 0 ? cols : 1;
            int lines = numItems / cols;
            lines = numItems % cols == 0 ? lines : lines + 1;

            // Check if we have too many items and need to ask the user
            if (_selectedItem == NO_ITEM_SELECTED && lines > Console.BufferHeight / 4 * 3)
            {
                string msg = String.Format("Want to see all {0} items in {1} lines? [Enter/Escape]",
                                           numItems, lines);
                WriteAt(0, RenderStartY + 1, msg, true);
                _selectedItem = ASKING_USER;
                return;
            }

            WriteAt(RenderStartX, RenderStartY, GetExpandedCommand(), true);
            int linesWithShownCommand = _maxWrittenY + 1 - RenderStartY;
            int showItemsOnLine = RenderStartY + linesWithShownCommand;

            if (numItems == 1)
            {
                return;
            }

            // print items
            // if we have more items than we can show, we need to calculate the scroll position
            int startShowItems = 0;
            int maxItemLines = bufferHeight - linesWithShownCommand;
            int selectionInLine = _selectedItem % lines;
            if (_selectedItem % lines > maxItemLines)
            {
                int skipLines = selectionInLine - maxItemLines;
                startShowItems = skipLines * cols;
            }
            int numShowItems = numItems - startShowItems;
            int shownLines = numShowItems / cols;
            shownLines = numShowItems % cols == 0 ? shownLines : shownLines + 1;
            shownLines = shownLines < maxItemLines ? shownLines : maxItemLines;

            // scroll buffer, adjust start position
            int scroll = MakeSureTextFitsInBuffer(showItemsOnLine + shownLines, "");
            showItemsOnLine -= scroll;

            // now show numShowItems items from startShowItems on from line showItemsOnLine
            int curItemLine = showItemsOnLine;
            for (int i = startShowItems; i < shownLines; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (i < numItems)
                    {
                        bool invertColors = i == _selectedItem; // highlight current selection
                        WriteAt((i % cols) * colwidth, curItemLine, _expandandedItems[i], false, invertColors);
                    }
                    i++;
                }
                curItemLine++;
            }
        }

        private void Clear()
        {
            if (_maxWrittenX <= RenderStartX && _maxWrittenY <= RenderStartY)
            {
                return;
            }
            int buffWidth = Console.BufferWidth;
            Console.SetCursorPosition(RenderStartX, RenderStartY);
            Console.Write("".PadLeft(buffWidth - RenderStartX));
            for (int i = 0; i < _maxWrittenY - RenderStartY; i++)
            {
                Console.SetCursorPosition(0, RenderStartY + i + 1);
                Console.Write("".PadLeft(buffWidth));
            }
            Console.SetCursorPosition(RenderStartX, RenderStartY);
            _maxWrittenX = RenderStartX;
            _maxWrittenY = RenderStartY;
        }

        private string GetCommonPrefix()
        {
            var commonPrefix = new StringBuilder();
            if (_expandandedItems.Length < 1)
            {
                return _softPrefix;
            }
            for (int charIdx = 0; ; charIdx++)
            {
                char curChar;
                int startComp = 0;
                if (charIdx < _softPrefix.Length)
                {
                    curChar = _softPrefix[charIdx];
                }
                else
                {
                    if (charIdx >= _expandandedItems[0].Length)
                    {
                        break;
                    }
                    curChar = _expandandedItems[0][charIdx];
                    startComp = 1;
                }
                bool doBreak = false;
                for (int i = startComp; i < _expandandedItems.Length; i++)
                {
                    if (i >= _expandandedItems[i].Length ||
                        _expandandedItems[i][charIdx] != curChar)
                    {
                        doBreak = true;
                        break;
                    }
                }
                if (doBreak)
                {
                    break;
                }
                commonPrefix.Append(curChar);
            }
            return commonPrefix.ToString();
        }

        private int MakeSureTextFitsInBuffer(int theoreticalPos, string str)
        {
            int buffHeight = Console.BufferHeight;
            int buffWidth = Console.BufferWidth;
            int endPos = theoreticalPos + str.Length / buffWidth;
            if (endPos < buffHeight)
            {
                return 0;
            }
            // TODO: check for auto-scrolling if cursor is in last line
            int doScroll = endPos - buffHeight + 1;
            int resetCursorX = Console.CursorLeft;
            int resetCursorY = Console.CursorTop - doScroll;
            Console.SetCursorPosition(buffWidth - 1, buffHeight - 1);
            for (int i = 0; i < endPos - buffHeight + 1; i++)
            {
                Console.WriteLine();
            }
            Console.SetCursorPosition(resetCursorX, resetCursorY);
            RenderStartY -= doScroll;
            _maxWrittenY -= doScroll;
            return doScroll;
        }

        private void WriteAt(int posX, int posY, string text, bool wrap)
        {
            WriteAt(posX, posY, text, wrap, false);
        }

        private void WriteAt(int posX, int posY, string text, bool wrap, bool colorInverse)
        {
            // backups
            int cursorX = Console.CursorLeft;
            int cursorY = Console.CursorTop;
            ConsoleColor fgColor = Console.ForegroundColor;
            ConsoleColor bgColor = Console.BackgroundColor;

            // scroll buffer
            string adjustToText = wrap ? text : "";
            int scrolledLines = MakeSureTextFitsInBuffer(posY, adjustToText);
            posY -= scrolledLines;
            cursorY -= scrolledLines;

            // actual write
            if (colorInverse)
            {
                Console.ForegroundColor = bgColor;
                Console.BackgroundColor = fgColor;
            }
            Console.SetCursorPosition(posX, posY);
            string writeText = wrap ? text : FittedText(text, Console.BufferWidth - posX);
            Console.Write(writeText);
            _maxWrittenX = Console.CursorLeft > _maxWrittenX ? Console.CursorLeft : _maxWrittenX;
            _maxWrittenY = Console.CursorTop > _maxWrittenY ? Console.CursorTop : _maxWrittenY;

            // reset
            if (colorInverse)
            {
                Console.ForegroundColor = fgColor;
                Console.BackgroundColor = bgColor;
            }
            Console.SetCursorPosition(cursorX, cursorY);
        }

        private string FittedText(string text, int width)
        {
            width -= 1; //newline char at EOL
            if (width < 1)
            {
                throw new Exception("Invalid Bufer width!");
            }
            if (width <= 3)
            {
                return "".PadRight(width, '.');
            }
            if (text.Length <= width)
            {
                return text.PadRight(width);
            }
            return text.Substring(0, width) + "...";
        }
    }

}

