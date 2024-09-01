namespace YMCL.Main.Public.Controls.TaskManage;

public class TaskManager
{
    public class TaskEntry
    {
        public PageTaskEntry _pageTaskEntry;
        public WindowTaskEntry _windowTaskEntry;

        public TaskEntry(string name, bool valueProgress = true, bool textProgress = true)
        {
            _pageTaskEntry = new PageTaskEntry(name, valueProgress, textProgress);
            _windowTaskEntry = new WindowTaskEntry(name, valueProgress);
        }

        public void UpdateTitle(string title)
        {
            _pageTaskEntry.UpdateTitle(title);
            _windowTaskEntry.UpdateTitle(title);
        }

        public void UpdateValueProgress(double progress)
        {
            _pageTaskEntry.UpdateValueProgress(progress);
            _windowTaskEntry.UpdateValueProgress(progress);
        }

        public void UpdateTextProgress(string text, bool includeTime = true)
        {
            _pageTaskEntry.UpdateTextProgress(text, includeTime);
            _windowTaskEntry.UpdateTextProgress(text, includeTime);
        }

        public void Finish()
        {
            _pageTaskEntry.Finish();
            _windowTaskEntry.Finish();
        }

        public void Destory()
        {
            _pageTaskEntry.Destory();
            _windowTaskEntry.Destory();
        }

        public void Show()
        {
            _windowTaskEntry.Show();
        }

        public void Activate()
        {
            _windowTaskEntry.Activate();
        }

        public void Hide()
        {
            _windowTaskEntry.Hide();
        }
    }
}