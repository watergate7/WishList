import React, { Component } from 'react';

class WishGallery extends Component {
    constructor(props) {
        super(props);
        this.state = {
            isLoading: false,
            wishItems: []
        };
    }

    componentDidMount() {
        // const fakeWishes = require('../mock_data/wishes.json');
        // this.setState({
        //     wishItems: fakeWishes
        // });

        this.setState({
            isLoading: true
        });

        fetch('../api/WishList/Get')
            .then(res => res.json())
            .then(
                result => {
                    this.setState({
                        isLoading: false,
                        wishItems: result
                    });
                }
            );
    }

    render() {
        if (this.state.isLoading) {
            return (<div className="wishLoading">
                Content is loading...
                </div>);
        }
        else if (this.state.wishItems && this.state.wishItems.length > 0) {
            const wishItems = this.state.wishItems;
            wishItems.forEach(wishItem => {
                wishItem.imgSrc = "data:image/png;base64," + wishItem.base64;
            });

            const wishList = wishItems.map(wishItem => {
                const item = (
                    <div className="wishItem">
                        <div className="inline imgContainer">
                            <img src={wishItem.imgSrc} />
                        </div>
                        <div className="inline">
                            <ul className="wishItemDetail">
                                <li>Name: {wishItem.name}</li>
                                <li>Type: {wishItem.type}</li>
                                <li>Price: {wishItem.price}</li>
                                <li>Currency: {wishItem.currency}</li>
                                <li>Brand: {wishItem.brand}</li>
                            </ul>
                        </div>
                    </div>
                );

                return item;
            });

            const container = (
                <div className="wishContainer">
                    {wishList}
                </div>
            );

            return container;
        }
        else {
            return (<div className="wishLoading">
                Your wishlist is empty, make a wish...
            </div>);
        }
    }
}

export default WishGallery;