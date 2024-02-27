using Engine;

namespace GameDemo.UI.Intro
{
    public class IntroLoader
    {
        private int currentForm = 0;
        private EngineCore engine;
        private IntroForm[] forms;
        private MainMenu mainMenu;

        public void Run(EngineCore engine, MainMenu mainMenu)
        {
            this.engine = engine;
            this.mainMenu = mainMenu;

            forms = new IntroForm[]
            {
                new DevLogoForm(engine, this)
            };

            LoadNextForm();
        }

        public void LoadNextForm()
        {
            if (currentForm >= forms.Length)
            {
                mainMenu.Show();
                return;
            }

            forms[currentForm].Show();
            ++currentForm;
        }
    }
}