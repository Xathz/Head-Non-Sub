<?php
/*
Nginx or other webservers need to be configured to serve all files
from the '/uploads' directory as plain text, UTF-8. e.g.

    location /headnonsub/uploads {
        add_header Content-Type "text/plain; charset=UTF-8";
    }

*/

// Upload key. Set to something random and long (at least 16).
// The key is required to be able to upload files.
$key = "";

if(!empty($_POST["key"])) {
    $uploadedKey = $_POST["key"];

    if ($uploadedKey != $key) {
        http_response_code(401);
        exit();
    }

    if(!empty($_FILES["file"])) {
        $allowed =  array("txt");
        $uploadedName = $_FILES["file"]["name"];

        $extension = pathinfo($uploadedName, PATHINFO_EXTENSION);
        if(!in_array($extension, $allowed) ) {
            http_response_code(415);
            exit();
        }

        $fileId = uniqid();
        $path = "uploads/" . $fileId;

        if(move_uploaded_file($_FILES["file"]["tmp_name"], $path)) {
            echo $fileId;
            http_response_code(200);
        } else {
            http_response_code(401);
        }

    } else {
        http_response_code(400);
    }
} else {
    http_response_code(401);
}

?>
