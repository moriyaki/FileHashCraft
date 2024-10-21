using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Models;
using FileHashCraft.Models.FileScan;
using FileHashCraft.ViewModels.SelectTargetPage;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.Tests
{
    /*
    public class ExtentionViewModelTest
    {
        [Fact]
        public void TestCheckBoxExtentionChecked()
        {
            // Mockの作成
            var messenger = new Mock<IMessenger>();
            var settingsService = new Mock<ISettingsService>();
            var extentionManager = new Mock<IExtentionManager>();
            var fileSearchCriteriaManager = new Mock<IFileSearchCriteriaManager>();

            // テスト対象のクラスに注入
            var extentionCheckBoxViewModel = new ExtensionCheckBoxViewModel(
                    messenger.Object,
                    settingsService.Object,
                    extentionManager.Object,
                    fileSearchCriteriaManager.Object);

            // テスト
            extentionCheckBoxViewModel.Initialize(".txt");
            extentionCheckBoxViewModel.IsChecked = true;
            Assert.True(extentionCheckBoxViewModel.IsChecked);
            extentionCheckBoxViewModel.IsChecked = false;
            Assert.False(extentionCheckBoxViewModel.IsChecked);

            // 結果
            messenger.Verify(m => m.Send(It.IsAny<ExtentionChechReflectToGroupMessage>(), It.IsAny<string>()), Times.Once);
            messenger.Verify(m => m.Send(It.IsAny<ExtentionCheckChangedToListBoxMessage>(), It.IsAny<string>()), Times.Once);
        }
    }
    */
}
