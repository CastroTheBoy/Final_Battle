using System;
using static System.Net.Mime.MediaTypeNames;

public static class UIManager
{
    static UIManager()
    {
        // ----------------------------------------------------------
        // Main game screen

        UIPane gameScreen = new UIPane();
        GameScreens.Add("Game", gameScreen);
        ActivePane = gameScreen;

        gameScreen.Elements.Add("OuterBorder", new UIElement(0, 0, 120, 30));
        gameScreen.Elements.Add("ActionBorder", new UIElement(0, 20, 81, 30));
        gameScreen.Elements.Add("LogBorder", new UIElement(80, 0, 120, 30));

        // Drawing outer, action and log block borders
        foreach (UIElement element in gameScreen.Elements.Values)
            UIMethods.BuildBorder(element);
        
        // Action block elements
        UIElement actionList = new UIElement(1, 21, 41, 29);
        gameScreen.Elements.Add("ActionList", actionList);
        ActionList = new UIListWriter(actionList);

        UIElement itemList = new UIElement(41, 21, 80, 29);
        gameScreen.Elements.Add("ItemList", itemList);
        ItemList = new UIListWriter(itemList);

        ActionList.SetList("Attack", "Kill", "Use item", "Equip gear", "Use gear");

        // Party blocks
        UIElement leftParty = new UIElement(1, 1, 40, 19);
        LeftParty = new UIListWriter(leftParty);
        gameScreen.Elements.Add("LeftParty", leftParty);

        UIElement rightParty = new UIElement(41, 1, 79, 19);
        RightParty = new UIListWriter(rightParty);
        gameScreen.Elements.Add("RightParty", rightParty);

        // Adding log
        UIElement battleLog = new UIElement(81,1,119,29);
        GameLog = new UIListWriter(battleLog);
        gameScreen.Elements.Add("BattleLog",battleLog);

        // ----------------------------------------------------------
        // Start screen

        UIPane startScreen = new UIPane();
        GameScreens.Add("Start", startScreen);

        startScreen.Elements.Add("OuterBorder", new UIElement(0, 0, 120, 30));

        // Drawing outer border
        foreach (UIElement element in startScreen.Elements.Values)
            UIMethods.BuildBorder(element, '*', '*');
    }

    public static Dictionary<string, UIPane> GameScreens = new Dictionary<string, UIPane>();
    public static UIPane ActivePane;
    public static UIListWriter GameLog;
    public static UIListWriter ActionList;
    public static UIListWriter ItemList;
    public static UIListWriter LeftParty;
    public static UIListWriter RightParty;

    public static ConsoleColor Foreground = ConsoleColor.White;
    public static ConsoleColor Background = ConsoleColor.Black;
    public static ConsoleColor Highlight = ConsoleColor.DarkRed;

    public static void Draw()
    {
        while (true)
        {
            Thread.Sleep(33); // 30 fps?
            ActivePane.Draw();
        }
    }
}

public class UIPane
{
    public Dictionary<string, UIElement> Elements { get; } = new Dictionary<string, UIElement>();

    public void Draw()
    {
        UIElement temp = new UIElement(0, 0, 120, 30);
        foreach (UIElement element in Elements.Values)
        {
            lock (element.Buffer.SyncRoot)
            {
                for (int i = 0; i < element.Buffer.GetLength(0); i++)
                    for (int j = 0; j < element.Buffer.GetLength(1); j++)
                    {
                        temp.Buffer[element.Left + i, element.Top + j] = element.Buffer[i, j].ShallowCopy();
                    }
            }
        }
        ToConsoleBuffer(temp.Buffer).Write();
    }

    private static ConsoleBuffer ToConsoleBuffer(ConsoleCharacter[,] arr)
    {
        ConsoleBuffer buffer = new ConsoleBuffer(0, 0, (short)arr.GetLength(0), (short)arr.GetLength(1));

        int x = 0;
        for (int j = 0; j < arr.GetLength(1); j++)
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                buffer.Add(x, Combiner(arr[i, j]), arr[i, j].Character);
                x++;
            }
        return buffer;
    }

    private static ushort Combiner(ConsoleCharacter cc)
    {
        return (ushort)((ushort)((ushort)cc.Background << 4) | (ushort)cc.Foreground);
    }
}

public class UIElement
{
    public ConsoleCharacter[,] Buffer { get; set; }
    public short Left;
    public short Right;
    public short Top;
    public short Bottom;

    public UIElement(short left, short top, short right, short bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
        Buffer = new ConsoleCharacter[right - left, bottom - top];
        for (int i = 0; i < Buffer.GetLength(0); i++)
            for (int j = 0; j < Buffer.GetLength(1); j++)
                Buffer[i, j] = new ConsoleCharacter();
    }

    public void Clear()
    {
        foreach (ConsoleCharacter cc in Buffer)
        {
            cc.Character = ' ';
        }
    }
}

public class ConsoleCharacterString
{
    private ConsoleCharacter[]? _array = null;
    public string Text { get; set; }
    public int Length { get => Text.Length; }
    private ConsoleColor _backgroundStandard;
    private ConsoleColor _backgroundHighlight = UIManager.Highlight;
    public ConsoleColor Background
    {
        get
        {
            return IsHighlighted ?_backgroundHighlight : _backgroundStandard;
        }
    }
    public ConsoleColor Foreground;
    public bool IsHighlighted = false;

    public ConsoleCharacterString(ConsoleCharacter[] array)
    {
        _array = array;
        string s = "";
        foreach (ConsoleCharacter c in _array)
            s += c.Character;
        Text = s;
    }

    public ConsoleCharacterString(string text, ConsoleColor backgroundStandard, ConsoleColor backgroundHighlighted, ConsoleColor foreground)
    {
        _backgroundHighlight = backgroundHighlighted;
        _backgroundStandard = backgroundStandard;
        Foreground = foreground;
        Text = text;
    }

    public ConsoleCharacterString(string text, ConsoleColor background, ConsoleColor foreground) : this(text, background, UIManager.Highlight, foreground) { }
    public ConsoleCharacterString(string text, ConsoleColor foreground) : this(text, UIManager.Background, foreground) { }
    public ConsoleCharacterString(string text) : this(text, UIManager.Foreground) { }

    public ConsoleCharacter[] ToArray()
    {
        if(_array != null)
        {
            foreach (ConsoleCharacter c in _array)
                c.Background = Background;
            return _array;
        }
        ConsoleCharacter[] arr = new ConsoleCharacter[Text.Length];
        for(int i = 0; i < Text.Length; i++)
            arr[i] = new ConsoleCharacter(Text[i],Background, Foreground);
        return arr;
    }
}

public class ConsoleCharacterBuilder
{
    private List<ConsoleCharacterString> _buffer = new();

    public void AddString(string text, ConsoleColor background, ConsoleColor foreground)
    {
        _buffer.Add(new ConsoleCharacterString(text, background, foreground));
    }
    public void AddString(string text, ConsoleColor foreground)
    {
        _buffer.Add(new ConsoleCharacterString(text, UIManager.Background, foreground));
    }
    public void AddString(string text)
    {
        _buffer.Add(new ConsoleCharacterString(text, UIManager.Background, UIManager.Foreground));
    }
    public void AddString(ConsoleCharacterString text)
    {
        _buffer.Add(text);
    }

    public ConsoleCharacterString Build()
    {
        int length = 0;
        foreach (ConsoleCharacterString c in _buffer)
            length += c.Text.Length;
        ConsoleCharacter[] arr = new ConsoleCharacter[length];
        int x = 0;
        foreach (ConsoleCharacterString c in _buffer)
        {
            c.ToArray().CopyTo(arr, x);
            x += c.Text.Length;
        }
        return new ConsoleCharacterString(arr);
    }

    public void Clear() => _buffer.Clear();
}

public class ConsoleCharacter
{
    public char Character { get; set; }
    public ConsoleColor Background {  get; set; }
    public ConsoleColor Foreground { get; set; }

    public ConsoleCharacter(char character, ConsoleColor background, ConsoleColor foreground)
    {
        Character = character;
        Foreground = foreground;
        Background = background;
    }

    public ConsoleCharacter() : this(' ', UIManager.Background, UIManager.Foreground) { }
    
    public ConsoleCharacter ShallowCopy()
    {
        return (ConsoleCharacter) MemberwiseClone();
    }
}

public class UIListWriter
{
    public int Count { get => _text.Count; }
    private List<ConsoleCharacterString> _text = new();
    private UIElement _element;
    private int _maxLineChars;
    private int _maxTotalChars;
    private int _lowerIndex = 0;
    private int _upperIndex = 0;
    private List<int> _highlighted = new();

    public UIListWriter(UIElement element)
    {
        _element = element;
        _maxLineChars = element.Right - element.Left;
        _maxTotalChars = (element.Bottom - element.Top) * (_maxLineChars);
    }

    public void ColorEntry(int index, bool highlight = true)
    {
        if (highlight)
        {
            if(!_highlighted.Contains(index)) _highlighted.Add(index);
            WriteList();
            return;
        }
        _highlighted.Remove(index);
        WriteList();
    }

    public void AddEntry(string text) => AddEntry(new ConsoleCharacterString(text));
    public void AddEntry(ConsoleCharacterString text)
    {
        _text.Add(text);
        _upperIndex = _text.Count - 1;
        _lowerIndex = GetMinMessageIndex(_upperIndex);
        WriteList();
    }
    public void SetList(params string[] text)
    {
        List<ConsoleCharacterString> list = new();
        foreach (string s in text)
            list.Add(new ConsoleCharacterString(s));
        _text = list;
        _highlighted.Clear();
        _lowerIndex = 0;
        _upperIndex = GetMaxMessageIndex(_lowerIndex);
        WriteList();
    }
    public List<string> GetList() => _text.Select(o => o.Text).ToList();
    
    public void ClearList()
    {
        _text.Clear();
        _highlighted.Clear();
        _lowerIndex = 0;
        _upperIndex = 0;
        _element.Clear();
    }
    public void ScrollList(bool up)
    {
        if (_text.Count == 0) return;
        if (up)
        {
            if (_lowerIndex == 0) return;
            _lowerIndex--;
            _upperIndex = GetMaxMessageIndex(_lowerIndex);
            WriteList();
            return;
        }
        if (_upperIndex + 1 == _text.Count) return;
        _upperIndex++;
        _lowerIndex = GetMinMessageIndex(_upperIndex);
        WriteList();
    }

    private void WriteList()
    {
        lock (_element.Buffer.SyncRoot)
        {
            _element.Clear();
            ConsoleCharacterString? s = null;
            ConsoleCharacter[]? arr = null;
            int newLineCounter = 0;
            int currentRowIndex;
            int currentMesage = 0;
            for (int i = _lowerIndex; i <= _upperIndex; i++)
            {
                currentRowIndex = 0;
                s = _text[i];
                s.IsHighlighted = _highlighted.Contains(i);
                arr = s.ToArray();
                for (int c = 0; c < arr.Length; c++)
                {
                    if (currentRowIndex >= _maxLineChars)
                    {
                        newLineCounter++;
                        currentRowIndex = 0;
                    }
                    _element.Buffer[currentRowIndex, newLineCounter] = arr[c].ShallowCopy();
                    currentRowIndex++;
                }
                newLineCounter++;
                currentMesage++;
            }
        }
    }

    private int GetMinMessageIndex(int maxIndex)
    {
        int runningTotal = 0;
        for (int i = maxIndex; i >= 0; i--)
        {
            runningTotal += (int)Math.Ceiling(((double)_text[i].Length / (double)_maxLineChars)) * _maxLineChars;
            if (runningTotal > _maxTotalChars)
                return i + 1;
        }
        return 0;
    }

    private int GetMaxMessageIndex(int minIndex)
    {
        int runningTotal = 0;
        for (int i = minIndex; i < _text.Count; i++)
        {
            runningTotal += (int)Math.Ceiling(((double)_text[i].Length / (double)_maxLineChars)) * _maxLineChars;
            if (runningTotal > _maxTotalChars)
                return i - 1;
        }
        return _text.Count - 1;
    }
}

public static class UIMethods
{
    public static void BuildBorder(UIElement element, char widthBorders = '|', char heightBorder = '-')
    {
        int width = element.Right - element.Left;
        int height = element.Bottom - element.Top;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (i == 0) { element.Buffer[i, j].Character = widthBorders; continue; }
                if (i == width - 1)
                { element.Buffer[i, j].Character = widthBorders; continue; }
                if (j == 0) { element.Buffer[i, j].Character = heightBorder; continue; }
                if (j == height - 1)
                { element.Buffer[i, j].Character = heightBorder; continue; }
            }
        }
    }
}

public static class UIListSelector
{
    public static int SelectFromList(UIListWriter list)
    {
        int index = 0;
        list.ColorEntry(index);

        while (true)
        {
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.Enter:
                    list.ColorEntry(index, false);
                    return index;
                case ConsoleKey.DownArrow:
                    list.ColorEntry(index, false);
                    if (index >= list.Count - 1)
                        index = 0;
                    else
                        index++;
                    list.ColorEntry(index);
                    continue;
                case ConsoleKey.UpArrow:
                    list.ColorEntry(index, false);
                    if (index == 0)
                        index = list.Count - 1;
                    else
                        index--;
                    list.ColorEntry(index);
                    continue;
            }
        }
    }
}