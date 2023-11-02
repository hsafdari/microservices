﻿using CartMicroservice.Model;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace CartMicroservice.Repository;

public class CartRepository : ICartRepository
{
    private readonly IMongoCollection<Cart> _col;

    public CartRepository(IMongoDatabase db)
    {
        _col = db.GetCollection<Cart>(Cart.DocumentName);
    }

    public IList<CartItem> GetCartItems(string userId) =>
        _col
        .Find(c => c.UserId == userId)
        .FirstOrDefault()?.CartItems ?? new List<CartItem>();

    public void InsertCartItem(string userId, CartItem cartItem)
    {
        var cart = _col.Find(c => c.UserId == userId).FirstOrDefault();
        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem> { cartItem }
            };
            _col.InsertOne(cart);
        }
        else
        {
            var ci = cart
                .CartItems
                .FirstOrDefault(ci => ci.CatalogItemId == cartItem.CatalogItemId);

            if (ci == null)
            {
                cart.CartItems.Add(cartItem);
            }
            else
            {
                ci.Quantity++;
            }

            var update = Builders<Cart>.Update
                .Set(c => c.CartItems, cart.CartItems);
            _col.UpdateOne(c => c.UserId == userId, update);
        }
    }

    public void UpdateCartItem(string userId, CartItem cartItem)
    {
        var cart = _col.Find(c => c.UserId == userId).FirstOrDefault();
        if (cart != null)
        {
            cart.CartItems.RemoveAll(ci => ci.CatalogItemId == cartItem.CatalogItemId);
            cart.CartItems.Add(cartItem);
            var update = Builders<Cart>.Update
                .Set(c => c.CartItems, cart.CartItems);
            _col.UpdateOne(c => c.UserId == userId, update);
        }
    }

    public void DeleteCartItem(string userId, string catalogItemId)
    {
        var cart = _col.Find(c => c.UserId == userId).FirstOrDefault();
        if (cart != null)
        {
            cart.CartItems.RemoveAll(ci => ci.CatalogItemId == catalogItemId);
            var update = Builders<Cart>.Update
                .Set(c => c.CartItems, cart.CartItems);
            _col.UpdateOne(c => c.UserId == userId, update);
        }
    }

    public void DeleteCatalogItem(string catalogItemId)
    {
        // Delete catalog item references from carts
        var carts = _col.Find(c => c.CartItems.Any(i => i.CatalogItemId == catalogItemId)).ToList();
        foreach (var cart in carts)
        {
            cart.CartItems.RemoveAll(i => i.CatalogItemId == catalogItemId);
            _col.UpdateOne(c => c.Id == cart.Id, Builders<Cart>.Update.Set(c => c.CartItems, cart.CartItems));
        }
    }
}