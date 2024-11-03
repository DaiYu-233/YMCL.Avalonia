namespace YMCL.Main.Public.Controls.TaskManage;

public class TaskManager
{
    public class TaskEntry
    {
        public PageTaskEntry _pageTaskEntry;
        public WindowTaskEntry? _windowTaskEntry;

        public TaskEntry(string name, bool valueProgress = true, bool textProgress = true)
        {
            _pageTaskEntry = new PageTaskEntry(name, valueProgress, textProgress);
            if (Const.Data.Setting.EnableDisplayIndependentTaskWindow)
                _windowTaskEntry = new WindowTaskEntry(name, valueProgress);
        }

        public void UpdateTitle(string title)
        {
            _pageTaskEntry.UpdateTitle(title);
            if (_windowTaskEntry != null) _windowTaskEntry.UpdateTitle(title);
        }

        public void UpdateValueProgress(double progress)
        {
            _pageTaskEntry.UpdateValueProgress(progress);
            if (_windowTaskEntry != null) _windowTaskEntry.UpdateValueProgress(progress);
        }

        public void UpdateTextProgress(string text, bool includeTime = true)
        {
            _pageTaskEntry.UpdateTextProgress(text, includeTime);
            if (_windowTaskEntry != null)
                _windowTaskEntry.UpdateTextProgress(text, includeTime);
        }

        public void Finish()
        {
            _pageTaskEntry.Finish();
            if (_windowTaskEntry != null) _windowTaskEntry.Finish();
        }

        public void Destory()
        {
            _pageTaskEntry.Destory();
            if (_windowTaskEntry != null) _windowTaskEntry.Destory();
        }

        public void Show()
        {
            if (_windowTaskEntry != null) _windowTaskEntry.Show();
        }

        public void Activate()
        {
            if (_windowTaskEntry != null) _windowTaskEntry.Activate();
        }

        public void Hide()
        {
            if (_windowTaskEntry != null) _windowTaskEntry.Hide();
        }
    }
}