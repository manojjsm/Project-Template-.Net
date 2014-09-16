/*this file will be responsible for creating modules in our applications.
we can consider module as a collection of services, directives and filters which is used
by our application*/

//initializing our angular modules
var app = angular.module('AngularAuthApp',['ngRoute','LocalStorageModule','angular-loading-bar']);

//configuration our routes
app.config(function ($routeProvider) {
    
    $routeProvider.when("/home", {
        controller: "homeController",
        templateUrl: "/app/views/home.html"
    });

    $routeProvider.when("/login", {
        controller: "loginController",
        templateUrl: "/app/views/login.html"
    });

    $routeProvider.when("/signup", {
        controller: "signupController",
        templateUrl: "/app/views/signup.html"
    });

    $routeProvider.when("/orders", {
        controller: "ordersController",
        templateUrl: "/app/views/orders.html"
    });

    $routeProvider.otherwise({ redirectTo: "/home" });

});

app.run(['authService', function (authService) {
    authService.fillAuthData();

}]);