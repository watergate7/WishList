import React, { Component } from 'react';

class WishGallery extends Component {
    render() {
        const fakeWishes = require('../mock_data/wishes.json');
        const wishItems = fakeWishes.map(wish => {
            const wishItem = (
                <div className="wishItem">
                    <p>wish.name</p>
                    <p>wish.type</p>
                    <p>wish.price</p>
                    <p>wish.brand</p>
                </div>
            );

            return wishItem;
        });

        const container = (
            <div className="wishContainer">
                {wishItems}
            </div>
        );

        return container;
    }
}

export default WishGallery;