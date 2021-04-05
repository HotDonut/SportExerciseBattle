using System;
using System.Collections.Generic;
using System.Text;

namespace SportsExerciseBattle.HelperObjects
{
    class ProfileToJSON
    {
        public string Name;
        public string Bio;
        public string Image;

        public ProfileToJSON(string _name, string _bio, string _image)
        {
            Name = _name;
            Bio = _bio;
            Image = _image;
        }
    }
}
