function subscribe() {
    const email = document.getElementById('email').value.trim();
    const successMessage = document.getElementById('success');
    const errorMessage = document.getElementById('error');

    // Hide previous messages
    successMessage.style.display = 'none';
    errorMessage.style.display = 'none';

    if (email === "") {
        errorMessage.style.display = 'block';
        return;
    }

    sendRequest(email);
}
function unsubscribe() {
    const email = document.getElementById('email').value.trim();
    const successMessage = document.getElementById('success');
    const errorMessage = document.getElementById('error');

    // Hide previous messages
    successMessage.style.display = 'none';
    errorMessage.style.display = 'none';

    if (email === "") {
        alert("Ange en giltig e-postadress för att avsluta prenumerationen.");
        return;
    }

    sendRequest(email);
}
function sendRequest(email) {
    const url = 'https://2bba-85-227-186-129.ngrok-free.app/Unsubscribe';
    const data = { email: email };

    fetch(url, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            document.getElementById('success').style.display = 'block';
        })
        .catch(error => {
            console.error('Error:', error);
            document.getElementById('error').style.display = 'block';
        });
}
function submitFeedback() {
    const subject = document.getElementById('subject').value;
    const name = document.getElementById('name').value;
    const succes = document.getElementById('succes');
    const err = document.getElementById('error');

    if (subject.trim() === "") {
        err.style.display = 'block';
        succes.style.display = 'none';
    } else {
        sendRequest(name, subject);
        subject.value = "";
        name.value = "";
        succes.style.display = 'block';
        err.style.display = 'none';
    }
}