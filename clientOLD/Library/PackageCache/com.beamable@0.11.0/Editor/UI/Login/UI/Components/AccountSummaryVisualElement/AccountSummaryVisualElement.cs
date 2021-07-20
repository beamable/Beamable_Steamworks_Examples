
using Beamable.Editor.Modules.Account;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
namespace Beamable.Editor.Login.UI.Components
{
   public class AccountSummaryVisualElement : LoginBaseComponent
   {
      public AccountSummaryVisualElement() : base(nameof(AccountSummaryVisualElement))
      {
      }

      private TextField _emailField;
      private TextField _aliasField;
      private TextField _roleField;
      private TextField _gameField;
      private TextField _idField;
      private TextField _psswordField;
      private Button _switchGameButton;

//      public override bool ShowHeader => false;

      public override string GetMessage()
      {
         return "You are logged in.";
      }

      public override void Refresh()
      {
         base.Refresh();

         _emailField = Root.Q<TextField>("email");
         _emailField.RegisterValueChangedCallback(evt => _emailField.SetValueWithoutNotify(Model?.CurrentUser?.email));

         _aliasField = Root.Q<TextField>("alias");
         _aliasField.RegisterValueChangedCallback(evt => _aliasField.SetValueWithoutNotify(Model?.Customer?.CidOrAlias));

         _roleField = Root.Q<TextField>("role");
         _roleField.RegisterValueChangedCallback(evt => _roleField.SetValueWithoutNotify(Model?.CurrentUser?.roleString));

         _gameField = Root.Q<TextField>("game");
         _gameField.RegisterValueChangedCallback(evt => _gameField.SetValueWithoutNotify(Model?.CurrentGame?.ProjectName));

         _switchGameButton = Root.Q<Button>("switchGame");
         _switchGameButton.clickable.clicked += Manager.GotoProjectSelectVisualElement;

         var resetPasswordButton = Root.Q<Button>("resetPassword");
         resetPasswordButton.clickable.clicked += Manager.GotoForgotPassword;

         var logoutButton = Root.Q<Button>("logout");
         logoutButton.clickable.clicked += () => Manager.Logout(Model);

         SetView();
         Model.OnStateChanged += _ => SetView();
      }

      private void SetView()
      {
         SetUserView();
         SetGameView();
      }

      private void SetUserView()
      {
         if (Model.CurrentUser == null) return;

         _emailField.SetValueWithoutNotify(Model.CurrentUser.email);
         _roleField.SetValueWithoutNotify(Model.CurrentUser.roleString);

      }

      private void SetGameView()
      {
         if (Model.CurrentGame == null || Model.CurrentCustomer == null) return;

         var shouldHideSwitchGame = Model.Games.Count == 1;
         if (shouldHideSwitchGame)
         {
            _switchGameButton.AddToClassList("hidden");
         }
         else
         {
            _switchGameButton.RemoveFromClassList("hidden");
         }

         _aliasField.SetValueWithoutNotify(Model.CurrentCustomer.Alias);
         _gameField.SetValueWithoutNotify(Model?.CurrentGame?.ProjectName);
      }

      private void LogOutButton_OnClicked()
      {
         Manager.Logout(Model);
      }
   }
}