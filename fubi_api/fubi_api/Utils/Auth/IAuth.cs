﻿namespace fubi_api.Utils.Auth
{
    public interface IAuth
    {
        string Hashear(string texto);

        bool ValidoContraHash(string password, string hashedPassword);
    }
}