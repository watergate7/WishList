import React, { Component } from 'react';
import { FormGroup, FormControl, ControlLabel, HelpBlock, Grid, Row, Col, Button } from 'react-bootstrap';

class MakeWish extends Component {
    constructor(props) {
        super(props);
        this.state = { name: '', type: '', brand: '', no: '', price: undefined };

        this.handleChange = this.handleChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
    }

    handleChange(event) {
        const name = event.target.id;
        const value = event.target.value;
        this.setState({ [name]: value });
    }

    handleSubmit(event) {
        fetch('../api/WishList/Add', {
            method: 'post',
            body: JSON.stringify(this.state)
        }).then(function (response) {
            alert(response.status);
        });

        event.preventDefault();
    }

    render() {
        const formInstance = (
            <form onSubmit={this.handleSubmit}>
                <Grid>
                    <Row className="show-grid">
                        <Col xs={4} md={4}>
                            <FieldGroup
                                id="name"
                                type="text"
                                label="Name"
                                value={this.state.name}
                                onChange={this.handleChange}
                                placeholder="Enter Name"
                            />
                        </Col>
                        <Col xs={4} md={4}>
                            <FieldGroup
                                id="type"
                                label="Type"
                                type="text"
                                value={this.state.type}
                                onChange={this.handleChange}
                                placeholder="Enter Type"
                            />
                        </Col>
                    </Row>
                    <Row className="show-grid">
                        <Col xs={4} md={4}>
                            <FieldGroup
                                id="brand"
                                label="Brand"
                                type="text"
                                value={this.state.brand}
                                onChange={this.handleChange}
                                placeholder="Enter Brand"
                            />
                        </Col>
                        <Col xs={4} md={4}>
                            <FieldGroup
                                id="no"
                                type="text"
                                label="Item Number"
                                value={this.state.no}
                                onChange={this.handleChange}
                                placeholder="Enter Item No."
                            />
                        </Col>
                        <Col xs={4} md={4}>
                            <FieldGroup
                                id="price"
                                type="number"
                                label="Estimated Price"
                                value={this.state.price}
                                onChange={this.handleChange}
                                placeholder="Enter Estimated Price"
                            />
                        </Col>
                    </Row>
                    <Row>
                        <Col xs={4} md={4}>
                            <FieldGroup
                                id="picture"
                                type="file"
                                label="Picture"
                                help="Upload a picture here."
                                accept="image/gif, image/jpeg, image/png"
                            />
                        </Col>
                    </Row>
                    <Row>
                        <Col xs={4} md={4}>
                            <Button type="submit">Submit</Button>
                        </Col>
                    </Row>
                </Grid>
            </form>);

        return formInstance;
    }
}

function FieldGroup({ id, label, help, ...props }) {
    return (
        <FormGroup controlId={id}>
            <ControlLabel>{label}</ControlLabel>
            <FormControl {...props} />
            {help && <HelpBlock>{help}</HelpBlock>}
        </FormGroup>
    );
}

export default MakeWish;



