AccountController
=========================================================================================================
    @GetMapping("/")
    public Package profile(@RequestHeader("Authorization") String token){
        try {
            Claims claims = Jwts.parser().setSigningKey(JWT_Key).parseClaimsJws(token).getBody();
            return new Package(0, "success", claims);
        } catch (Exception e){
            return new Package(404, e.getMessage(), null);
        }
    }

    @PatchMapping("/")
    public Package updateProfile(@RequestParam("name") String name,
                                 @RequestParam("file") Optional<MultipartFile> multipartFile,
                                 @RequestHeader("Authorization") String token) {
        try {
            // Parse the authentication token to extract the username
            Claims claims = Jwts.parser().setSigningKey(JWT_Key).parseClaimsJws(token).getBody();
            String username = claims.get("username", String.class);
            // Retrieve the user's profile information from the database
            UserModel user = db.findByUsername(username);
            // Update the image URL if necessary
            if (multipartFile.isPresent()) {
                String imageUrl = firebase.uploadImage(multipartFile.get());
                user.setImage(imageUrl);
            }
            // Update the username if necessary
            if (!user.getName().equals(name))
                user.setName(name);
            // Save the updated profile information
            UserModel result = db.save(user);
            // Return a success message with the updated user information
            return new Package(0, "Update profile successfully", result);
        }catch (Exception e){
            return new Package(404, e.getMessage(), null);
        }
    }
    
    @PostMapping("/change-password")
    public Package changePassword(
            @RequestParam("currentPassword") String currentPassword,
            @RequestParam("newPassword") String newPassword,
            @RequestParam("confirmPassword") String confirmPassword,
            @RequestHeader(name = "Authorization") String token){
        try{
            if (validateToken(token)) {
                Claims claims = Jwts.parser().setSigningKey(JWT_Key).parseClaimsJws(token).getBody();
                String username = claims.get("username", String.class);

                UserModel user = db.findByUsername(username);
                if (user != null && passwordEndcoder.matches(currentPassword, user.getPassword())) {
                    // Update user password
                    user.setPassword(passwordEndcoder.encode(newPassword));
                    UserModel result = db.save(user);

                    return new Package(0, "Changing password successfully", result);
                } else {
                    return new Package(401, "Incorrect current password", null);
                }
            } else {
                return new Package(401, "Invalid token", null);
            }
        }catch (Exception e){
            return new Package(404, e.getMessage(), null);
        }
    }
    
