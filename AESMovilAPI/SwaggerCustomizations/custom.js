window.onload = function () {
    // Replace the logo
    const logo = document.querySelector('.swagger-ui .topbar .topbar-wrapper .link img');
    if (logo) {
        logo.src = '/swagger-ui/custom/custom-logo.png';
        logo.alt = 'AES El Salvador';
    }
};