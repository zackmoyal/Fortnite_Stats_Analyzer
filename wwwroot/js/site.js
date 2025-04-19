// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', function () {
    const form = document.querySelector('form');
    const usernameInput = document.querySelector('#username');
    const allowedUsernames = ["ZbiZniZ", "Tfue", "Bugha", "Ninja"]; // hardcoded suggestions

    if (form && usernameInput) {
        form.addEventListener('submit', function (e) {
            const entered = usernameInput.value.trim();

            if (!entered) {
                alert("Please enter a username.");
                e.preventDefault();
                return;
            }

            const exactMatch = allowedUsernames.find(name => name === entered);
            const closeMatch = allowedUsernames.find(name => name.toLowerCase() === entered.toLowerCase());

            if (!exactMatch && closeMatch) {
                alert(`Username '${entered}' is incorrect. Did you mean '${closeMatch}'? Please enter it with exact casing.`);
                e.preventDefault();
                return;
            }
        });
    }
});
