import React, { Component } from 'react';

class WishGallery extends Component {
    constructor(props) {
        super(props);
        this.state = {
            wishItems: []
        };
    }

    componentDidMount() {
        // const fakeWishes = require('../mock_data/wishes.json');
        // this.setState({
        //     wishItems: fakeWishes
        // });

        fetch('../api/WishList/Get')
            .then(res => res.json())
            .then(
                result => {
                    this.setState({
                        wishItems: result
                    });
                }
            );
    }

    render() {
        if (this.state.wishItems && this.state.wishItems.length > 0) {
            const wishItems = this.state.wishItems;
            wishItems.forEach(wishItem => {
                wishItem.imgSrc = "data:image/png;base64," + wishItem.base64;
            });

            const wishList = wishItems.map(wishItem => {
                const item = (
                    <div className="wishItem">
                        <div className="inline">
                            <p>Name: {wishItem.name}</p>
                            <p>Type: {wishItem.type}</p>
                            <p>Price: {wishItem.price}</p>
                            <p>Brand: {wishItem.brand}</p>
                        </div>
                        <div className="inline">
                            <img width="200px" src={wishItem.imgSrc} />
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
                Content is loading...
            </div>);
        }
    }
}

export default WishGallery;