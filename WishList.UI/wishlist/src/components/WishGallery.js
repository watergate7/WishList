import React, { Component } from 'react';
import { Button } from 'react-bootstrap';

class WishGallery extends Component {
    constructor(props) {
        super(props);
        this.state = {
            isLoading: false,
            wishItems: []
        };
    }

    componentDidMount() {
        const fakeWishes = require('../mock_data/wishes.json');
        this.setState({
            wishItems: fakeWishes
        });

        // this.setState({
        //     isLoading: true
        // });

        // fetch('../api/WishList/Get')
        //     .then(res => res.json())
        //     .then(
        //         result => {
        //             this.setState({
        //                 isLoading: false,
        //                 wishItems: result
        //             });
        //         }
        //     );
    }

    onDelete(id) {
        // fetch('../api/WishList/Delete?id=' + id)
        //     .then(
        //         result => {
        //             var index = this.state.wishItems.findIndex(x => x.ID == id);
        //             this.state.wishItems.splice(index, 1);

        //             this.setState({
        //                 wishItems: this.state.wishItems
        //             });
        //         }
        //     )
        //     .catch(
        //         error => {
        //             alert(error);
        //         }
        //     )
    }

    onComplete(id) {
        // fetch('../api/WishList/Delete?id=' + id)
        //     .then(
        //         result => {
        //             var index = this.state.wishItems.findIndex(x => x.ID == id);
        //             this.state.wishItems.splice(index, 1);

        //             this.setState({
        //                 wishItems: this.state.wishItems
        //             });
        //         }
        //     )
        //     .catch(
        //         error => {
        //             alert(error);
        //         }
        //     )
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
                        <div className="displayBar">
                            <div className="imgContainer">
                                <img src={wishItem.imgSrc} />
                            </div>
                            <div className="wishItemDetail">
                                <div style={{ color: !wishItem.status || wishItem.status.toLowerCase() == 'active' ? '#1E90FF' : '#228B22' }}>Status: {wishItem.status}</div>
                                <ul style={{ listStyle: 'none', paddingLeft: '0px' }}>
                                    <li>Name: {wishItem.name}</li>
                                    <li>Type: {wishItem.type}</li>
                                    <li>Brand: {wishItem.brand}</li>
                                    <li>Number: {wishItem.no}</li>
                                    <li>Price: {wishItem.price}</li>
                                    <li>Currency: {wishItem.currency}</li>
                                    <li>Comment: {wishItem.comment}</li>
                                </ul>
                                <div className="feedback">
                                    {!!wishItem.feedback ?
                                        wishItem.feedback : null
                                    }
                                </div>
                            </div>
                        </div>
                        <div className="operationBar">
                            <Button bsStyle="success" onClick={this.onComplete(wishItem.ID)}>Complete</Button>
                            <Button bsStyle="danger" onClick={this.onDelete(wishItem.ID)}>Delete</Button>
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