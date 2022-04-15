using AuthenticationServices;
using DustyPig.Mobile.CrossPlatform.SocialLogin;
using DustyPig.Mobile.iOS.CrossPlatform.SocialLogin;
using Foundation;
using System;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(AppleLoginClientImplementation))]
namespace DustyPig.Mobile.iOS.CrossPlatform.SocialLogin
{
    public class AppleLoginClientImplementation : NSObject, IAppleLoginClient, IASAuthorizationControllerDelegate, IASAuthorizationControllerPresentationContextProviding
    {
        TaskCompletionSource<string> _taskCompletionSource;


        public Task<string> LoginAsync()
        {
            _taskCompletionSource = new TaskCompletionSource<string>();

            var provider = new ASAuthorizationAppleIdProvider();
            
            var req = provider.CreateRequest();
            req.RequestedScopes = new[] { ASAuthorizationScope.FullName, ASAuthorizationScope.Email };

            var controller = new ASAuthorizationController(new[] { req })
            {
                Delegate = this,
                PresentationContextProvider = this
            };
            controller.PerformRequests();
                        
            return _taskCompletionSource.Task;
        }


        public UIWindow GetPresentationAnchor(ASAuthorizationController controller)
            => UIApplication.SharedApplication.KeyWindow;

        [Export("authorizationController:didCompleteWithAuthorization:")]
        public void DidComplete(ASAuthorizationController controller, ASAuthorization authorization)
        {
            var creds = authorization.GetCredential<ASAuthorizationAppleIdCredential>();
            _taskCompletionSource?.TrySetResult(creds.IdentityToken.ToString());
        }

        [Export("authorizationController:didCompleteWithError:")]
        public void DidComplete(ASAuthorizationController controller, NSError error)
        {
            if (error.Code == 1001)
                _taskCompletionSource?.TrySetCanceled();
            else
                _taskCompletionSource?.TrySetException(new Exception(error.LocalizedDescription));
        }
    }
}