/*Angular JS service will be responsible for signing up new users, log-in/logout
registered users and store the generated token in client local storage so this 
token can be sent with each request to access secure resources on the back-end API*/

'use strict';

app.factory('authService', ['$http', '$q', 'localStorageService', function ($http, $q, localStorageService) {
    var serviceBase = 'http://localhost:54074/'
    var authServiceFactory = {};

    var _authentication = {
        usAuth: false,
        userName:""
    };

    //this method will return a promise that will be resolved at the controller
    var _saveRegistration = function (registration) {
        _logOut();

        return $http.post(serviceBase + 'api/account/register', registration).then(function (response) {
            return response;
        });
    };


    /*this endpoint will validate the credentials passed and if they are
    valid it will return an access_token.*/
    /*we will store this token to our localstorage and for the successiding request
    we will read this token value and sent it in the "Authorization" header
    of the HTTP request*/
    var _login = function (loginData) {

        var data = "grant_type=password&username=" + loginData.userName + "&password=" + loginData.password;
        
        var deffered = $q.defer();
        
        //verify the username and password by our WebAPI
        //we have set the content-type "x-www-form-urlencoded" and sent our data as a string not as JSON Object
        $http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {

            //if username and password is valid save our accestoken in the browsers localstorage
            localStorageService.set('authorizationData', { token: response.access_token, userName: loginData.userName });

            _authentication.isAuth = true;
            _authentication.userName = loginData.userName;

            deferred.resolve(response);
        }).error(function (err, status) {
            _logOut();
            deferred.reject(err);

        });

        return deferred.promise;
    };

    var _logOut = function () {
        localStorageService.remove('authorizationData');

        _authentication.isAuth = false;
        _authentication.userName = "";
    };

    var _fillAuthData = function () {

        var authData = localStorageService.get('authorizationData');
        if (authData)
        {
            _authentication.isAuth = true;
            _authentication.userName = authData.userName;
        }

    };


    authServiceFactory.saveRegistration = _saveRegistration;
    authServiceFactory.login = _login;
    authServiceFactory.logOut = _logOut;
    authServiceFactory.fillAuthData = _fillAuthData;
    authServiceFactory.authentication = _authentication;

    return authServiceFactory;

}]);