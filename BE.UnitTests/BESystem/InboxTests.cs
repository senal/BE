using System;
using System.Collections.Generic;
using BE.Services;
using BE.Services.Exceptions;
using Moq;
using NUnit.Framework;

namespace BE.UnitTests.BESystem
{
    [TestFixture]
    public class InboxTests
    {

        private InboxMailService _sut;
        private Mock<IConfigurationManager> _configManager;
        private Mock<IInboxMailDbManager> _inboxDbManager;
        private Mock<IMailManager> _mailManager; 
        private const string MailBox = "inbox";
        private const string MailPassword = "!@#1234";
        private const string MailServer = "192.168.1.1";

        [SetUp]
        public void SetUp()
        {
            _configManager = new Mock<IConfigurationManager>();
            _inboxDbManager = new Mock<IInboxMailDbManager>();
            _mailManager = new Mock<IMailManager>();
            _sut = new InboxMailService(_configManager.Object, _inboxDbManager.Object, _mailManager.Object);

        }

        [Test]
        public void InboxRefresh_ShouldConfigureSettingToRefreshMailBox()
        {
            //Arrange
            FillMails();
            EnableReadMailSettings();
            MailManagerConnect(true);
            //Act
            _sut.RefreshInbox();

            //Assert
            _configManager.Verify(m => m.Read<bool>(It.IsAny<string>()), Times.Once());

        }
        
        [Test]
        public void InboxRefresh_ShouldExitIfNoConfigurationValueFound()
        {
            //Arrange
            _configManager.Setup(m => m.Read<bool>("InboxRefresh")).Returns(false);


            //Act
            _sut.RefreshInbox();

            //Assert
            _configManager.Verify(m => m.Read<bool>("InboxRefresh"),Times.Once);
            _configManager.Verify(m => m.Read<string>(It.IsAny<string>()),Times.Exactly(0));
            
        }

        [Test]
        public void InboxRefresh_ShouldReadMailSettingsIfConfiguredToRefreshMailbox()
        {
            //Arrange
            _configManager.Setup(m => m.Read<bool>("InboxRefresh")).Returns(true);
            EnableReadMailSettings();
            MailManagerConnect(true);
            FillMails();

            //Act
            _sut.RefreshInbox();

            //Assert
            _configManager.Verify(m => m.Read<bool>("InboxRefresh"), Times.Once);
            _configManager.Verify(m => m.Read<string>(It.IsAny<string>()), Times.Exactly(3));

        }

        [Test]
        [ExpectedException(typeof (InboxMailServiceException),
            ExpectedMessage = "Unable to connect to Email Server.  Please check your connection settings.")]
        public void InboxRefresh_ShouldExitWithExceptionIfMailSettingsEmpty()
        {
            //Arrange
            _configManager.Setup(m => m.Read<bool>("InboxRefresh")).Returns(true);
            EmptyMailSettings();

            //Act
            _sut.RefreshInbox();

            //Assert
            _configManager.Verify(m => m.Read<bool>("InboxRefresh"), Times.Once);

        }

        [Test]
        public void InboxRefresh_ShouldNotThrowAnyExceptionIfAllMailSettingsAreAvailable()
        {
            //Arrange
            EnableReadMailSettings();
            MailManagerConnect(true);
            FillMails();

            //Act
            _sut.RefreshInbox();

            //Assert
            _configManager.Verify(m => m.Read<bool>("InboxRefresh"), Times.Once);
        }

        [Test]
        public void InboxRefresh_ShouldDeleteAndCreate_tblEmailInbox_In_Database()
        {
            //Arrange
            EnableReadMailSettings();
            MailManagerConnect(true);
            FillMails();

            //Act
            _sut.RefreshInbox();

            //Assert
            _inboxDbManager.Verify(m=>m.DeleteAndCreateInboxTable(),Times.Once);
            
        }

        [Test]
        public void InboxRefresh_ShouldConnectToMailServer()
        {
            //Arrange
            EnableReadMailSettings();
            MailManagerConnect(true);
            FillMails();

            //Act
            _sut.RefreshInbox();

            //Assert
            _mailManager.Verify(m=>m.Connect(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()),Times.Once);
            
        }

        [Test]
        [ExpectedException(typeof(InboxMailServiceException), ExpectedMessage = "Unable to connect to Email Server.  Please check and verify connection settings.")]
        public void InboxRefresh_ShouldExitWithExceptionWhenConnectionFailedToMailServer()
        {
            //Arrange
            MailManagerConnect(false);
            EnableReadMailSettings();

            //Act
            _sut.RefreshInbox();

        }

        [Test]
        public void InboxRefresh_ShouldReadMailsFromGivenInbox()
        {
            //Arrange
            EnableReadMailSettings();
            MailManagerConnect(true);
            FillMails();
            
            //Act
            _sut.RefreshInbox();

            //Assert
            _mailManager.Verify(m => m.GetMails(), Times.Once);
        }

        [Test]
        public void InboxRefresh_ShoulAbleGetMessageById()
        {
            //Arrange
            IMail mailMessage = new Email() { Id = 1234, Sender = new Sender() { Address = "ranga@gmail.com", Name = "Ranga" } };
            _mailManager.Setup(m => m.GetMessage(It.IsAny<long>()))
                .Returns(mailMessage);
            EnableReadMailSettings();
            MailManagerConnect(true);
            FillMails();

            //Act
            _sut.RefreshInbox();

            //Assert
            _mailManager.Verify(m=>m.GetMessage(It.IsAny<long>()), Times.AtLeastOnce);

        }

        private void FillMails()
        {
            var mails = new List<IMailEntry> { new MailEntryItem() { Id = 1234 }, new MailEntryItem() { Id = 5678 } };

            _mailManager.Setup(m => m.GetMails())
                .Returns(mails);
        }

        private void EnableReadMailSettings()
        {
            _configManager.Setup(m => m.Read<bool>("InboxRefresh")).Returns(true);
            _configManager.Setup(m => m.Read<string>("EmailInbox")).Returns(MailBox);
            _configManager.Setup(m => m.Read<string>("EmailPassword")).Returns(MailPassword);
            _configManager.Setup(m => m.Read<string>("EmailServer")).Returns(MailServer);
        }

        private void EmptyMailSettings()
        {
            _configManager.Setup(m => m.Read<string>("EmailInbox")).Returns(string.Empty);
            _configManager.Setup(m => m.Read<string>("EmailPassword")).Returns(string.Empty);
            _configManager.Setup(m => m.Read<string>("EmailServer")).Returns(string.Empty);
        }

        private void MailManagerConnect(bool connect)
        {
            _mailManager.Setup(
               m => m.Connect(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()))
               .Returns(connect);
        }


        [TearDown]
        public void TearDown()
        {
        }
    }

    public class Email : IMail
    {
        public long Id { get; set; }
        public ISender Sender { get; set; }
    }

    public class Sender : ISender
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
