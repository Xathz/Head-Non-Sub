<?php
/*
Nginx or other webservers need to be configured to serve certian files.
Text files need to be UTF-8.

    location "/headnonsub/uploads/" {
        deny all;
        
        location ~* "\.(txt)$" {
            allow all;
            add_header Content-Type "text/plain; charset=UTF-8";
        }

        location ~* "\.(png)$" {
            allow all;
            add_header Content-Type "image/png";
        }

        location ~* "\.(gif)$" {
            allow all;
            add_header Content-Type "image/gif";
        }
    }

*/

// Upload key. Set to something random and long (at least 16).
// The key is required to be able to upload files.
$key = "";

if (!empty($_POST["key"])) {
    $uploadedKey = $_POST["key"];

    if ($uploadedKey != $key) {
        http_response_code(401);
        exit();
    }

    if (!empty($_FILES["file"])) {
        $allowed =  array("txt", "png", "gif");
        $uploadedName = $_FILES["file"]["name"];
        $uploadedExtension = pathinfo($uploadedName, PATHINFO_EXTENSION);

        if (!in_array($uploadedExtension, $allowed)) {
            http_response_code(415);
            exit();
        }

        $fileId = bin2hex(random_bytes(16));
        $path = "uploads/" . $fileId . "." . $uploadedExtension;

        while (true) {
            if (!file_exists($path)) { break; }

            $fileId = bin2hex(random_bytes(16));
            $path = "uploads/" . $fileId . "." . $uploadedExtension;
        }

        if (move_uploaded_file($_FILES["file"]["tmp_name"], $path)) {
            echo $fileId . "." . $uploadedExtension;
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
