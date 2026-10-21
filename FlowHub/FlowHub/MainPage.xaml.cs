namespace FlowHub
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private void Button_Clicked(object sender, TouchEventArgs e)
        {

        }

        private void Button_LongPressed(object sender, TouchEventArgs e)
        {

        }

        private void Button_Pressed(object sender, TouchEventArgs e)
        {

        }
    }

}
