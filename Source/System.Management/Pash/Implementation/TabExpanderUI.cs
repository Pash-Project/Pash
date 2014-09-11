using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
        private bool _userWasAsked;

        private int _maxWrittenX;
        private int _maxWrittenY;
        private int _lastRenderNumRows;
        private bool _abortedOrAccepted;

        public int RenderStartX { get; private set; }
        public int RenderStartY { get; private set; }
        public bool Running { get; private set; }

        public TabExpansionEvent TabExpansionEvent;

        public TabExpanderUI()
        {
            Running = false;
            _abortedOrAccepted = true;
        }

        public bool HandleKey(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Modifiers != 0)
            {
                return false;
            }

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
            case ConsoleKey.DownArrow:
            case ConsoleKey.UpArrow:
            case ConsoleKey.LeftArrow:
            case ConsoleKey.RightArrow:
                return ChangeSelection(keyInfo.Key);
            default:
                return false;
            }
        }

        private bool ChangeSelection(ConsoleKey key)
        {
            if (_selectedItem < NO_ITEM_SELECTED || _lastRenderNumRows < 1)
            {
                return false; // just in case
            }
            // nothing selected? then select first
            if (_selectedItem == NO_ITEM_SELECTED)
            {
                _selectedItem = 0;
                return true;
            }
            // otherwise select depending on last rendering
            if (key == ConsoleKey.UpArrow && _selectedItem > 0)
            {
                _selectedItem--;
            }
            else if (key == ConsoleKey.DownArrow && _selectedItem < _expandandedItems.Length - 1)
            {
                _selectedItem++;
            }
            else if (key == ConsoleKey.LeftArrow)
            {
                int newsel = _selectedItem - _lastRenderNumRows;
                if (newsel >= 0)
                {
                    _selectedItem = newsel;
                }
            }
            else if (key == ConsoleKey.RightArrow)
            {
                int newsel = _selectedItem + _lastRenderNumRows;
                if (newsel <= _expandandedItems.Length -1)
                {
                    _selectedItem = newsel;
                }
            }
            return true;
        }

        public void Start(string prefix)
        {
            Running = true;
            if (!_abortedOrAccepted && prefix.Equals(GetExpandedCommand()))
            {
                return;
            }
            _abortedOrAccepted = false;
            if (prefix == null)
            {
                prefix = ""; // just in case
            }
            // split the last (quoted) word from the whole prefix as the soft/replacable prefix
            int splitPos = prefix.LastUnquotedIndexOf(' ') + 1;
            _hardPrefix = prefix.Substring(0, splitPos);
            _softPrefix = prefix.Substring(splitPos);
            _selectedItem = NOT_INITIALIZED;
            _userWasAsked = false;
        }

        public void Accept()
        {
            // finished
            _abortedOrAccepted = true;
            Running = false;
        }

        public void ChooseNext()
        {
            bool selectNextItem = true;
            if (_selectedItem == ASKING_USER)
            {
                _userWasAsked = true;
            }
            else if (_selectedItem == NOT_INITIALIZED)
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
                _abortedOrAccepted = true;
                _selectedItem = NOT_INITIALIZED;
            }
        }

        public string GetExpandedCommand()
        {
            string chosenPrefix;
            if (_selectedItem < NO_ITEM_SELECTED)
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
            return _hardPrefix + chosenPrefix;
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

            int colwidth = _expandandedItems.Max(item => item.Length) + 2;
            if (colwidth >= bufferWidth)
            {
                colwidth = bufferWidth - 1;
            }
            int cols = bufferWidth / colwidth;
            cols = cols > 0 ? cols : 1;
            int rows = numItems / cols;
            rows = numItems % cols == 0 ? rows : rows + 1;

            // Check if we have too many items and need to ask the user
            if (_selectedItem == NO_ITEM_SELECTED && 
                rows > Console.BufferHeight / 4 * 3 && 
                !_userWasAsked)
            {
                string msg = String.Format("Want to see all {0} items in {1} lines? [Enter/Escape]",
                                           numItems, rows);
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

            _lastRenderNumRows = rows;

            // print items
            // if we have more items than we can show, we need to calculate the scroll position
            int startShowRow = 0;
            int numShownLines = rows;
            int maxItemLines = bufferHeight - linesWithShownCommand;
            int selectionInLine = _selectedItem % rows;
            if (selectionInLine >= maxItemLines - linesWithShownCommand)
            {
                startShowRow = selectionInLine - maxItemLines + 1;
                numShownLines = rows - startShowRow;
            }
            numShownLines = numShownLines > maxItemLines ? maxItemLines : numShownLines;
            int lastShownLine = numShownLines + startShowRow -1;

            // scroll buffer, adjust start position
            int scroll = MakeSureTextFitsInBuffer(showItemsOnLine + numShownLines, "");
            showItemsOnLine -= scroll;

            // now show items
            for (int i = 0; i < numItems; i++)
            {
                int col = i / rows;
                int row = i % rows;
                if (row < startShowRow || row > lastShownLine)
                {
                    continue;
                }
                row -= startShowRow; // adjust row to relative row shown
                bool highlight = i == _selectedItem;
                WriteAt(col * colwidth, row + showItemsOnLine, _expandandedItems[i], false, highlight);
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
                if (charIdx >= _expandandedItems[0].Length)
                {
                    break;
                }
                char curChar = _expandandedItems[0][charIdx];

                bool doBreak = false;
                for (int i = 1; i < _expandandedItems.Length; i++)
                {
                    if (charIdx >= _expandandedItems[i].Length ||
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
            return commonPrefix.Length > 0 ? commonPrefix.ToString() : _softPrefix;
        }

        private int MakeSureTextFitsInBuffer(int theoreticalPos, string str)
        {
            int buffHeight = Console.BufferHeight;
            int buffWidth = Console.BufferWidth;
            int endPos = theoreticalPos + str.Length / buffWidth + 1;
            if (endPos < buffHeight)
            {
                return 0;
            }
            // TODO: check for auto-scrolling if cursor is in last line
            int doScroll = endPos - buffHeight;
            doScroll = doScroll > Console.CursorTop ? Console.CursorTop : doScroll; //limit to be able to reset cursor
            int resetCursorX = Console.CursorLeft;
            int resetCursorY = Console.CursorTop - doScroll;
            SaveSetCursorPosition(buffWidth - 1, buffHeight - 1);
            for (int i = 0; i < doScroll; i++)
            {
                Console.WriteLine();
            }
            SaveSetCursorPosition(resetCursorX, resetCursorY);
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
            SaveSetCursorPosition(posX, posY);
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
            SaveSetCursorPosition(cursorX, cursorY);
        }

        private void SaveSetCursorPosition(int x, int y)
        {
            x = x < 0 ? 0 : x;
            x = x >= Console.BufferWidth ? Console.BufferWidth - 1 : x;
            y = y < 0 ? 0 : y;
            y = y >= Console.BufferHeight ? Console.BufferHeight - 1 : y;
            Console.SetCursorPosition(x, y);
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

