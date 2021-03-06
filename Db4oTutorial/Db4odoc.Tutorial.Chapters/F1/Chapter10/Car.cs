/* This file is part of the db4o object database http://www.db4o.com

Copyright (C) 2004 - 2011  Versant Corporation http://www.versant.com

db4o is free software; you can redistribute it and/or modify it under
the terms of version 3 of the GNU General Public License as published
by the Free Software Foundation.

db4o is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or
FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
for more details.

You should have received a copy of the GNU General Public License along
with this program.  If not, see http://www.gnu.org/licenses/. */
namespace Db4odoc.Tutorial.F1.Chapter10
{
    public class Car
    {
        string _model;
        Pilot _pilot;
        
        public Car(string model, Pilot pilot)
        {
            _model = model;
            _pilot = pilot;
        }
      
        public Pilot Pilot
        {
            get
            {
                return _pilot;
            }
            
            set
            {
                _pilot = value;
            }
        }
        
        public string Model         
        {
            get
            {
                return _model;
            }
        }
        
        override public string ToString()
        {
			return string.Format("{0}[{1}]", _model, _pilot);
        }
    }
}
