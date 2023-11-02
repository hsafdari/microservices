﻿using CartMicroservice.Model;
using System.Collections.Generic;

namespace CartMicroservice.Repository;

public interface ICartRepository
{
    IList<CartItem> GetCartItems(string userId);
    void InsertCartItem(string userId, CartItem cartItem);
    void UpdateCartItem(string userId, CartItem cartItem);
    void DeleteCartItem(string userId, string cartItemId);
    void DeleteCatalogItem(string catalogItemId);
}