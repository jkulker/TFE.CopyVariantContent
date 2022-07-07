angular.module("umbraco").controller("TFE.Translating", function ($scope, $http, navigationService, contentEditingHelper) {
    $scope.busy = false;
    $scope.success = false;
    $scope.error = false;
    $scope.errorMessage = "";
    $scope.includeChilderen = false;

    $scope.createVariants = async function (nodeId) {
        $scope.busy = true;

        var result =  await $http({
            method: "GET",
            params: {
                nodeId: nodeId,
                includeChilderen: $scope.includeChilderen
            },
            url: "backoffice/Variants/Variant/CreateContentVariants/"
        });

        if (result.data.IsSucces) {
            $scope.success = true;
        }
        else {
            $scope.errorMessage = $scope.data.Error.Message;
            $scope.error = true;
        }

        $scope.busy = false;
    }
    $scope.closeDialog = function () {
        navigationService.hideDialog();
        location.reload();
    };
    $scope.includeChilderenClick = function () {
        $scope.includeChilderen = !$scope.includeChilderen
    };
});